using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AllSub.Common.Extensions;
using AllSub.Common.Models;
using AllSub.CommonCore.Interfaces.EventBus;
using AllSub.CommonCore.Services;
using AllSub.EventBusRabbitMQ;
using AllSub.WebMVC.Data;
using AllSub.WebMVC.Hubs;
using AllSub.WebMVC.Services;
using AllSub.OAuth.Vk;
using RabbitMQ.Client;
using System;
using Autofac.Core;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.IO;

namespace AllSub.WebMVC
{
    public static class Program
    {
        public static /*async Task*/ void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureElasticSerilog("WebMVC");

            var cultureInfoRu = new CultureInfo("ru");
            cultureInfoRu.NumberFormat.CurrencySymbol = "\u20bd";   // unicode ruble symbol

            CultureInfo.DefaultThreadCurrentCulture = cultureInfoRu;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfoRu;

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { cultureInfoRu };
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.DefaultRequestCulture = new RequestCulture(cultureInfoRu, cultureInfoRu);
                options.ApplyCurrentCultureToResponseHeaders = true;
            });

            // Attempt to avoid VS debugger issue
            EnsureFile("WebMVC.pfx");
            EnsureFile("appsecrets.json");

            builder.Configuration.AddJsonFile("/app/appsecrets.json");  // TODO: refactor. Use true secrets

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddControllersWithViews();
            builder.Services.AddEventBus(builder.Configuration);
            builder.Services.AddSignalR();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            AddAuthentication(builder);
            AddServices(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // TODO: Handle migrations in production
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // TODO: The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapHub<SearchHub>("/searchHub");

            app.UseEventBus();

            app.Run();
        }
        private static void EnsureFile(string fileName)
        {
            if (!File.Exists($"/app/{fileName}"))
            {
                File.Copy($"/src/{fileName}", $"/app/{fileName}");
            }
        }
        private static void AddAuthentication(WebApplicationBuilder builder) 
        {
            var configuration = builder.Configuration;

            builder.Services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = configuration["Secrets:Google:ClientId"] ?? string.Empty;
                    googleOptions.ClientSecret = configuration["Secrets:Google:ClientSecret"] ?? string.Empty;
                    googleOptions.SaveTokens = true;
                    googleOptions.AccessType = "offline";


                    googleOptions.Events.OnCreatingTicket = ctx =>
                    {
                        // Store tokens at this point
                        List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
                        var temp = ctx.Properties.GetTokenValue("access_token");
                        var temp1 = ctx.Properties.GetTokenValue("token_type");
                        var temp2 = ctx.Properties.GetTokenValue("expires_at");

                        return Task.CompletedTask;
                    };
                })
                .AddVk(vkOptions =>
                {
                    vkOptions.ClientId = configuration["Secrets:Vk:ClientId"] ?? string.Empty;
                    vkOptions.ClientSecret = configuration["Secrets:Vk:ClientSecret"] ?? string.Empty;
                    vkOptions.SaveTokens = true;
                    vkOptions.ApiVersion = "5.131";

                    vkOptions.Scope.Add("groups");
                    vkOptions.Scope.Add("video");
                    vkOptions.Scope.Add("offline");

                    vkOptions.Events.OnCreatingTicket = ctx =>
                    {
                        // Store tokens at this point
                        List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
                        var temp = ctx.Properties.GetTokenValue("access_token");
                        var temp1 = ctx.Properties.GetTokenValue("token_type");
                        var temp2 = ctx.Properties.GetTokenValue("expires_at");

                        return Task.CompletedTask;
                    };
                });

        }

        private static void UseEventBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<SearchCompletedEvent, IIntegrationEventHandler<SearchCompletedEvent>>();
        }

        private static void AddServices(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient<IVkServiceIntegration, VkServiceIntegration>();
            builder.Services.AddHttpClient<IYtServiceIntegration, YtServiceIntegration>();
            builder.Services.AddHttpClient<ITestAdServiceIntegration, TestAdServiceIntegration>();

            builder.Services.AddTransient<INotificationCache, NotificationCache>();
            builder.Services.AddTransient<INotificationService,  NotificationService>();
        }

        private static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var retryCount = 5;
            if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
            {
                if (!int.TryParse(configuration["EventBusRetryCount"], out retryCount))
                {
                    retryCount = 5;
                }
            }

            services.AddSingleton<IEventBus, EventBusRabbitMQ.EventBusRabbitMQ>(sp =>
            {
                var subscriptionClientName = configuration["SubscriptionClientName"];
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ.EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusRabbitMQ.EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddTransient< IIntegrationEventHandler<SearchCompletedEvent>, SearchCompletedEventHandler >();

            services.AddSingleton<IRabbitMQPersistentConnection>(sp => {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = configuration["EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                {
                    factory.UserName = configuration["EventBusUserName"];
                }

                if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                {
                    factory.Password = configuration["EventBusPassword"];
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

            return services;
        }
    }
}
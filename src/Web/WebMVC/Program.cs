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
using RabbitMQ.Client;
using System;
using Autofac.Core;

namespace AllSub.WebMVC
{
    public static class Program
    {
        public static /*async Task*/ void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureElasticSerilog("WebMVC");
            // Attempt to avoid the secrets issue
            try
            {
                builder.Configuration.AddJsonFile("/app/appsecrets.json");  // TODO: refactor
            }
            catch
            {
                builder.Configuration.AddJsonFile("/src/appsecrets.json");  // TODO: refactor
            }

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddSignalR();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddEventBus(builder.Configuration);

            var configuration = builder.Configuration;
            builder.Services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Secrets:Google:ClientId"] ?? string.Empty;
                googleOptions.ClientSecret = configuration["Secrets:Google:ClientSecret"] ?? string.Empty;
            });

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapHub<SearchHub>("/searchHub");

            app.UseEventBus();

            // Apply database migration automatically. Note that this approach is not
            // recommended for production scenarios. Consider generating SQL scripts from
            // migrations instead.
            //using (var scope = app.Services.CreateScope())
            //{
            //    await SeedData.EnsureSeedData(scope, app.Configuration, app.Logger);
            //}

            app.Run();
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
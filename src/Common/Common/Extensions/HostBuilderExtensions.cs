using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace AllSub.Common.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureElasticSerilog(this IHostBuilder builder, string moduleName)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
	        var configuration = new ConfigurationBuilder()
		        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		        .AddJsonFile(
			        $"appsettings.{environment}.json",
			        optional: true)
		        .Build();

            var uriSink = new Uri(configuration["ElasticConfiguration:Uri"] ?? "http://ek:9200");
	        var elSinkOptions =  new ElasticsearchSinkOptions(uriSink)
	        {
		        AutoRegisterTemplate = true,
		        IndexFormat = $"allsub-{moduleName}-{environment.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
	        };

	        var loggerConfig = new LoggerConfiguration()
		        .Enrich.FromLogContext()
		        .Enrich.WithMachineName()
		        .WriteTo.Debug()
		        .WriteTo.Console()
		        .WriteTo.Elasticsearch(elSinkOptions)
		        .Enrich.WithProperty("Environment", environment)
		        .ReadFrom.Configuration(configuration);
            Log.Logger = loggerConfig.CreateLogger();
            
			builder.UseSerilog();

            return builder;
        }
    }
}

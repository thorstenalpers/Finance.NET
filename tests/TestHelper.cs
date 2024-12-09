using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFinance.Extensions;
using NetFinance.Tests.IntegrationTests;

namespace NetFinance.Tests;

internal static class TestHelper
{
	public static ServiceProvider SetUpServiceProvider()
	{
		var services = new ServiceCollection();

		var cfgBuilder = new ConfigurationBuilder();
		cfgBuilder.AddUserSecrets<AlphaVantageTests>();
		cfgBuilder.AddEnvironmentVariables();
		var cfg = cfgBuilder.Build();

		services.AddSingleton<IConfiguration>(cfg);
		services.AddNetFinance(new NetFinanceConfiguration
		{
			Http_Timeout = 20,
			Http_Retries = 3,
			AlphaVantageApiKey = cfg["NetFinance:AlphaVantageApiKey"]
		});
		services.AddLogging(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Information);
			builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
			//builder.AddFilter("NetFinance", LogLevel.Debug);

			builder.AddSimpleConsole(options =>
			{
				options.UseUtcTimestamp = true;
				options.SingleLine = true;
				options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
			});
		});
		return services.BuildServiceProvider();
	}
}

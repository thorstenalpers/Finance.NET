using DotNetFinance.Extensions;
using DotNetFinance.Tests.IntegrationTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetFinance.Tests;

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
		services.AddFinanceServices(new DotNetFinanceConfiguration
		{
			HttpTimeout = 20,
			HttpRetries = 3,
			AlphaVantageApiKey = cfg["DotNetFinance:AlphaVantageApiKey"]
		});
		services.AddLogging(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Information);
			builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
			builder.AddFilter("Finance.Net", LogLevel.Information);

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

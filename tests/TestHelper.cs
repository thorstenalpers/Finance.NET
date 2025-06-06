﻿using Finance.Net.Extensions;
using Finance.Net.Tests.IntegrationTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Finance.Net.Tests;

internal static class TestHelper
{
    public static ServiceProvider SetUpServiceProvider()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.Indented
        };

        var services = new ServiceCollection();
        var cfgBuilder = new ConfigurationBuilder();
        cfgBuilder.AddUserSecrets<AlphaVantageTests>();
        cfgBuilder.AddEnvironmentVariables();
        var cfg = cfgBuilder.Build();

        services.AddSingleton<IConfiguration>(cfg);
        services.AddFinanceNet(new FinanceNetConfiguration
        {
            HttpTimeout = 3,
            HttpRetryCount = 3,
            AlphaVantageApiKey = cfg["FinanceNet:AlphaVantageApiKey"]
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

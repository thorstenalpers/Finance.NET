using System;
using System.Net.Http;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;

namespace Finance.Net.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Finance.NET Service
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="cfg">Optional: Default values to configure Finance.NET. <see cref="FinanceNetConfiguration"/> ></param>
    public static void AddFinanceNet(this IServiceCollection services, FinanceNetConfiguration? cfg = null)
    {
        cfg ??= new FinanceNetConfiguration();

        services.Configure<FinanceNetConfiguration>(opt =>
        {
            opt.HttpRetryCount = cfg.HttpRetryCount;
            opt.HttpTimeout = cfg.HttpTimeout;
            opt.AlphaVantageApiKey = cfg.AlphaVantageApiKey;
            opt.DatahubIoDownloadUrlSP500Symbols = cfg.DatahubIoDownloadUrlSP500Symbols;
            opt.DatahubIoDownloadUrlNasdaqListedSymbols = cfg.DatahubIoDownloadUrlNasdaqListedSymbols;
            opt.YahooCookieExpirationTime = cfg.YahooCookieExpirationTime;
        });

        services.AddSingleton<IReadOnlyPolicyRegistry<string>, PolicyRegistry>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(Constants.DefaultHttpRetryPolicy);
            var options = serviceProvider.GetRequiredService<IOptions<FinanceNetConfiguration>>();

            return new PolicyRegistry
            {
                {
                    Constants.DefaultHttpRetryPolicy, Policy
                        .Handle<HttpRequestException>()
                        .WaitAndRetryAsync(
                            options.Value.HttpRetryCount,
                            retryAttempt => TimeSpan.FromSeconds( retryAttempt), // delayed retry
                            (exception, timeSpan, retryCount, _) => logger.LogWarning("Retry {RetryCount} after {TimeSpan} due to {Exception}.", retryCount, timeSpan, exception))
                }
            };
        });

        services.AddSingleton<IYahooSessionState, YahooSessionState>();
        services.AddSingleton<IYahooSessionManager, YahooSessionManager>();

        services.AddScoped<IYahooFinanceService, YahooFinanceService>();
        services.AddScoped<IXetraService, XetraService>();
        services.AddScoped<IAlphaVantageService, AlphaVantageService>();
        services.AddScoped<IDatahubIoService, DatahubIoService>();

        services.AddHttpClient(Constants.YahooHttpClientName)
            .ConfigureHttpClient((provider, client) =>
            {
                var session = provider.GetRequiredService<IYahooSessionManager>();
                var userAgent = session.GetUserAgent();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                client.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeout);
            })
            .ConfigurePrimaryHttpMessageHandler((provider) =>
            {
                var sessionState = provider.GetRequiredService<IYahooSessionState>();
                var handler = new HttpClientHandler
                {
                    CookieContainer = sessionState.GetCookieContainer(),
                    UseCookies = true,
                };
                return handler;
            });

        services.AddHttpClient(Constants.XetraHttpClientName)
            .ConfigureHttpClient(client =>
            {
                var userAgent = Helper.CreateRandomUserAgent();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                client.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeout);
            });

        services.AddHttpClient(Constants.AlphaVantageHttpClientName)
            .ConfigureHttpClient(client =>
            {
                var userAgent = Helper.CreateRandomUserAgent();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                client.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeout);
            });

        services.AddHttpClient(Constants.DatahubIoHttpClientName)
            .ConfigureHttpClient(client =>
            {
                var userAgent = Helper.CreateRandomUserAgent();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                client.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeout);
            });
    }
}
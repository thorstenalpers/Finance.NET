using System;
using System.Net.Http;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Finance.Net.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Configures Finance.NET Service
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="configuration">Optional: Default values to configure Finance.NET. <see cref="FinanceNetConfiguration"/> ></param>
	public static void AddFinanceServices(this IServiceCollection services, FinanceNetConfiguration? cfg = null)
	{
		cfg ??= new FinanceNetConfiguration();

		services.Configure<FinanceNetConfiguration>(opt =>
		{
			opt.HttpRetries = cfg.HttpRetries;
			opt.HttpTimeout = cfg.HttpTimeout;
			opt.AlphaVantageApiKey = cfg.AlphaVantageApiKey;
			opt.AlphaVantageApiUrl = cfg.AlphaVantageApiUrl;
			opt.XetraDownloadUrlInstruments = cfg.XetraDownloadUrlInstruments;
			opt.DatahubIoDownloadUrlSP500Symbols = cfg.DatahubIoDownloadUrlSP500Symbols;
			opt.DatahubIoDownloadUrlNasdaqListedSymbols = cfg.DatahubIoDownloadUrlNasdaqListedSymbols;
			opt.YahooBaseUrlQuoteHtml = cfg.YahooBaseUrlQuoteHtml;
			opt.YahooBaseUrlAuthentication = cfg.YahooBaseUrlAuthentication;
			opt.YahooBaseUrlCrumbApi = cfg.YahooBaseUrlCrumbApi;
			opt.YahooBaseUrlQuoteApi = cfg.YahooBaseUrlQuoteApi;
			opt.YahooCookieRefreshTime = cfg.YahooCookieRefreshTime;
			opt.YahooBaseUrlConsent = cfg.YahooBaseUrlConsent;
			opt.YahooBaseUrlConsentCollect = cfg.YahooBaseUrlConsentCollect;
		});

		services.AddSingleton<IYahooSessionState, YahooSessionState>();
		services.AddSingleton<IYahooSessionManager, YahooSessionManager>();

		services.AddScoped<IYahooService, YahooService>();
		services.AddScoped<IXetraService, XetraService>();
		services.AddScoped<IAlphaVantageService, AlphaVantageService>();
		services.AddScoped<IDatahubIoService, DatahubIoService>();

		services.AddHttpClient(cfg.YahooHttpClientName)
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

		services.AddHttpClient(cfg.XetraHttpClientName)
			.ConfigureHttpClient(client =>
			{
				var userAgent = Helper.CreateRandomUserAgent();
				client.DefaultRequestHeaders.Add("User-Agent", userAgent);
				client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
				client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
				client.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeout);
			});

		services.AddHttpClient(cfg.AlphaVantageHttpClientName)
			.ConfigureHttpClient(client =>
			{
				var userAgent = Helper.CreateRandomUserAgent();
				client.DefaultRequestHeaders.Add("User-Agent", userAgent);
				client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
				client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
				client.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeout);
			});

		services.AddHttpClient(cfg.DatahubIoHttpClientName)
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
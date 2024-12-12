using System;
using System.Linq;
using System.Net.Http;
using DotNetFinance.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace DotNetFinance.Tests.Extensions;

[TestFixture]
[Category("UnitTests")]
public class ServiceCollectionExtensionsTests
{


	[SetUp]
	public void SetUp()
	{

	}

	[Test]
	public void AddFinanceServices_WithCfg_Added()
	{
		// Arrange
		var services = new ServiceCollection();
		var cfg = new DotNetFinanceConfiguration()
		{
			AlphaVantageApiKey = "xxx",
			AlphaVantageApiUrl = "https://www.google1.de",
			DatahubIoDownloadUrlNasdaqListedSymbols = "https://www.google2.de",
			DatahubIoDownloadUrlSP500Symbols = "https://www.google3.de",
			XetraDownloadUrlInstruments = "https://www.google4.de",
			YahooBaseUrlAuthentication = "https://www.google5.de",
			YahooBaseUrlCrumbApi = "https://www.google6.de",
			YahooBaseUrlQuoteHtml = "https://www.google7.de",
			YahooBaseUrlQuoteApi = "https://www.google8.de",
			YahooBaseUrlConsent = "https://www.google9.de",
			HttpRetries = 100,
			HttpTimeout = 1000,
		};

		// Act
		services.AddFinanceServices(cfg);

		// Assert
		var provider = services.BuildServiceProvider();

		using var scope = provider.CreateScope();
		var resolvedCfg = scope.ServiceProvider.GetService<IOptions<DotNetFinanceConfiguration>>().Value;
		var clientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

		// Assert
		Assert.That(resolvedCfg.AlphaVantageApiKey, Is.EqualTo(cfg.AlphaVantageApiKey));
		Assert.That(resolvedCfg.HttpRetries, Is.EqualTo(cfg.HttpRetries));
		Assert.That(resolvedCfg.YahooBaseUrlQuoteHtml, Is.EqualTo(cfg.YahooBaseUrlQuoteHtml));
		Assert.That(resolvedCfg.YahooBaseUrlAuthentication, Is.EqualTo(cfg.YahooBaseUrlAuthentication));
		Assert.That(resolvedCfg.YahooBaseUrlCrumbApi, Is.EqualTo(cfg.YahooBaseUrlCrumbApi));
		Assert.That(resolvedCfg.YahooBaseUrlQuoteApi, Is.EqualTo(cfg.YahooBaseUrlQuoteApi));
		Assert.That(resolvedCfg.HttpTimeout, Is.EqualTo(cfg.HttpTimeout));
		Assert.That(resolvedCfg.XetraDownloadUrlInstruments, Is.EqualTo(cfg.XetraDownloadUrlInstruments));
		Assert.That(resolvedCfg.XetraDownloadUrlInstruments, Is.EqualTo(cfg.XetraDownloadUrlInstruments));
		Assert.That(resolvedCfg.DatahubIoDownloadUrlSP500Symbols, Is.EqualTo(cfg.DatahubIoDownloadUrlSP500Symbols));
		Assert.That(resolvedCfg.DatahubIoDownloadUrlNasdaqListedSymbols, Is.EqualTo(cfg.DatahubIoDownloadUrlNasdaqListedSymbols));
		Assert.That(resolvedCfg.AlphaVantageApiUrl, Is.EqualTo(cfg.AlphaVantageApiUrl));

		var clientDatahubIo = clientFactory.CreateClient(cfg.DatahubIoHttpClientName);
		var userAgent = clientDatahubIo.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
		Assert.That(userAgent, Is.Not.Empty);
		Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientDatahubIo.Timeout));

		var clientAlphaVantage = clientFactory.CreateClient(cfg.AlphaVantageHttpClientName);
		userAgent = clientAlphaVantage.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
		Assert.That(userAgent, Is.Not.Empty);
		Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientAlphaVantage.Timeout));

		var clientXetra = clientFactory.CreateClient(cfg.XetraHttpClientName);
		userAgent = clientXetra.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
		Assert.That(userAgent, Is.Not.Empty);
		Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientXetra.Timeout));

		var clientYahoo = clientFactory.CreateClient(cfg.YahooHttpClientName);
		Assert.That(clientYahoo, Is.Not.Null);
		userAgent = clientYahoo.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
		Assert.That(userAgent, Is.Not.Empty);
		Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientYahoo.Timeout));
	}
}

using System;
using System.Linq;
using System.Net.Http;
using Finance.Net.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Finance.Net.Tests.Extensions;

[TestFixture]
[Category("Unit")]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddFinanceServices_WithCfg_Added()
    {
        // Arrange
        var services = new ServiceCollection();
        var cfg = new FinanceNetConfiguration()
        {
            AlphaVantageApiKey = "MyKey",
            DatahubIoDownloadUrlNasdaqListedSymbols = "https://www.google2.de",
            DatahubIoDownloadUrlSP500Symbols = "https://www.google3.de",
            YahooCookieExpirationTime = 7,
            HttpRetryCount = 100,
            HttpTimeout = 1000,
        };

        // Act
        services.AddFinanceNet(cfg);

        // Assert
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var resolvedCfg = scope.ServiceProvider.GetService<IOptions<FinanceNetConfiguration>>().Value;
        var clientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

        // Assert
        Assert.That(resolvedCfg.AlphaVantageApiKey, Is.EqualTo(cfg.AlphaVantageApiKey));
        Assert.That(resolvedCfg.HttpRetryCount, Is.EqualTo(cfg.HttpRetryCount));
        Assert.That(resolvedCfg.HttpTimeout, Is.EqualTo(cfg.HttpTimeout));
        Assert.That(resolvedCfg.DatahubIoDownloadUrlSP500Symbols, Is.EqualTo(cfg.DatahubIoDownloadUrlSP500Symbols));
        Assert.That(resolvedCfg.DatahubIoDownloadUrlNasdaqListedSymbols, Is.EqualTo(cfg.DatahubIoDownloadUrlNasdaqListedSymbols));

        var clientDatahubIo = clientFactory.CreateClient(Constants.DatahubIoHttpClientName);
        var userAgent = clientDatahubIo.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
        Assert.That(userAgent, Is.Not.Empty);
        Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientDatahubIo.Timeout));

        var clientAlphaVantage = clientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        userAgent = clientAlphaVantage.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
        Assert.That(userAgent, Is.Not.Empty);
        Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientAlphaVantage.Timeout));

        var clientXetra = clientFactory.CreateClient(Constants.XetraHttpClientName);
        userAgent = clientXetra.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
        Assert.That(userAgent, Is.Not.Empty);
        Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientXetra.Timeout));

        var clientYahoo = clientFactory.CreateClient(Constants.YahooHttpClientName);
        Assert.That(clientYahoo, Is.Not.Null);
        userAgent = clientYahoo.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
        Assert.That(userAgent, Is.Not.Empty);
        Assert.That(TimeSpan.FromSeconds(cfg.HttpTimeout), Is.EqualTo(clientYahoo.Timeout));
    }
}

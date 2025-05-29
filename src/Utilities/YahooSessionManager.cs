using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Finance.Net.Utilities;

internal class YahooSessionManager(ILogger<YahooSessionManager> logger,
    IHttpClientFactory httpClientFactory,
    IYahooSessionState sessionState,
    IReadOnlyPolicyRegistry<string> policyRegistry) : IYahooSessionManager
{
    private readonly ILogger<YahooSessionManager> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IYahooSessionState _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly AsyncPolicy _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

    public string? GetApiCrumb()
    {
        return _sessionState.GetCrumb();
    }

    public string GetUserAgent()
    {
        return _sessionState.GetUserAgent();
    }

    public IEnumerable<Cookie> GetCookies()
    {
        return _sessionState.GetCookieContainer().GetCookies(new Uri(Constants.YahooBaseUrl)).Cast<Cookie>();
    }

    public string GetCookieNames()
    {
        return string.Join(",", _sessionState.GetCookieContainer().GetCookies(new Uri(Constants.YahooBaseUrl)).Cast<Cookie>().Select(e => e.Name));
    }

    public async Task RefreshSessionAsync(CancellationToken token = default)
    {
        _logger.LogDebug("cookieNames={Cookies}", GetCookieNames());

        if (_sessionState.IsValid())
        {
            return;
        }
        await Semaphore.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var crumb = await CreateApiCookiesAndCrumb(token).ConfigureAwait(false);
                _sessionState.SetCrumb(crumb, DateTime.UtcNow);
                await CreateUiCookies(token).ConfigureAwait(false);
                if (!_sessionState.IsValid())
                {
                    throw new FinanceNetException("cannot fetch Yahoo credentials");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No Yahoo session created", ex);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task<string> CreateApiCookiesAndCrumb(CancellationToken token)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        httpClient.DefaultRequestHeaders.Add(Constants.HeaderAccept, "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

        var response = await httpClient.GetAsync(Constants.YahooAuthenticationUrl.ToLowerInvariant(), token).ConfigureAwait(false);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, Constants.YahooCrumbApiUrl.ToLowerInvariant());
        var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        requestMessage.Headers.Add("Cookie", cookieHeader);
        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

        response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
        var crumb = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(crumb))
        {
            throw new FinanceNetException("Unable to retrieve Yahoo crumb.");
        }
        if (crumb.Contains("Too Many Requests"))
        {
            throw new FinanceNetException("Too Many Requests.");
        }
        if (_sessionState.GetCookieContainer().Count < 3)
        {
            throw new FinanceNetException("Unable to get api cookies.");
        }
        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

        _logger.LogDebug("cookieNames={Cookies}", GetCookieNames());
        _logger.LogDebug("_crumb= {Crumb}", crumb);
        _logger.LogInformation("API Session established successfully");
        return crumb;
    }

    private async Task CreateUiCookies(CancellationToken token)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        httpClient.DefaultRequestHeaders.Add(Constants.HeaderAccept, "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

        // get consent
        var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, Constants.YahooBaseUrl, token).ConfigureAwait(false);
        await DeclineConsentAsync(document, token).ConfigureAwait(false);
    }

    public async Task DeclineConsentAsync(IHtmlDocument document, CancellationToken token)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        httpClient.DefaultRequestHeaders.Add(Constants.HeaderAccept, "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

        var csrfTokenNode = document.QuerySelector("input[name='csrfToken']");
        var sessionIdNode = document.QuerySelector("input[name='sessionId']");
        if (csrfTokenNode == null || sessionIdNode == null)
        {
            var responseConsent = await httpClient.GetAsync(Constants.YahooBaseUrl, token).ConfigureAwait(false);
            responseConsent.EnsureSuccessStatusCode();

            // no EU consent, call from coming outside of EU
            if (_sessionState.GetCookieContainer().Count >= 3)
            {
                _logger.LogInformation("UI Session established successfully");
                return;
            }
            else
            {
                throw new FinanceNetException($"Unable to create ui cookies, cnt={_sessionState.GetCookieContainer()?.Count}");
            }
        }
        var csrfToken = csrfTokenNode.GetAttribute("value");
        var sessionId = sessionIdNode.GetAttribute("value");
        if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(sessionId))
        {
            throw new FinanceNetException($"Unable to retrieve csrfToken and sessionId, cnt={_sessionState.GetCookieContainer().Count}");
        }

        // reject consent
        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
        var postData = new List<KeyValuePair<string, string?>>
                        {
                            new("csrfToken", csrfToken),
                            new("sessionId", sessionId),
                            new("originalDoneUrl", Constants.YahooBaseUrl ),
                            new("namespace", "yahoo"),
                        };
        foreach (var value in new List<string> { "reject", "reject" })
        {
            postData.Add(new("reject", value));
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.YahooConsentCollectUrl}?sessionId={sessionId}")
        {
            Content = new FormUrlEncodedContent(postData)
        };
        requestMessage.Headers.Add(Constants.HeaderAccept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        var response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
        _logger.LogDebug("cookieNames={Cookies}", GetCookieNames());
        response.EnsureSuccessStatusCode();
        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

        // finalize
        response = await httpClient.GetAsync(Constants.YahooBaseUrl, token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
        if (_sessionState.GetCookieContainer().Count >= 3)
        {
            _logger.LogInformation("UI Session established successfully");
        }
        else
        {
            throw new FinanceNetException($"Unable to create ui cookies, cnt={_sessionState.GetCookieContainer()?.Count}");
        }
    }

    public void InvalidateSession()
    {
        _sessionState.InvalidateSession();
    }
}

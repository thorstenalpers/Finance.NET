using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finance.Net.Utilities;

internal class YahooSessionManager(IHttpClientFactory httpClientFactory,
	ILogger<IYahooSessionManager> logger,
								   IOptions<FinanceNetConfiguration> options,
								   IYahooSessionState sessionState) : IYahooSessionManager
{
	private readonly ILogger<IYahooSessionManager> _logger = logger;
	private readonly IYahooSessionState _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
	private readonly FinanceNetConfiguration _options = options.Value ?? throw new ArgumentNullException(nameof(options));
	private static readonly SemaphoreSlim _semaphore = new(1, 1);
	private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

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
		var cookies = _sessionState.GetCookieContainer().GetCookies(new Uri(Constants.YahooBaseUrlHtml)).Cast<Cookie>();
		return cookies;
	}

	public async Task RefreshSessionAsync(CancellationToken token = default, bool forceRefresh = false)
	{
		var cookies = GetCookies();
		_logger.LogDebug(() => $"cookieNames= {string.Join(", ", cookies.Cast<Cookie>().Select(cookie => cookie.Name))}");

		if (forceRefresh)
		{
			_sessionState.InvalidateSession();
		}
		if (_sessionState.IsValid())
		{
			return;
		}

		await _semaphore.WaitAsync(token).ConfigureAwait(false);

		if (_sessionState.IsValid())
		{
			return;
		}

		try
		{
			for (int attempt = 1; attempt <= 5; attempt++)
			{
				try
				{

					var crumb = await CreateApiCookiesAndCrumb(token).ConfigureAwait(false);
					_sessionState.SetCrumb(crumb);
					await CreateUiCookies(token).ConfigureAwait(false);
					if (!_sessionState.IsValid())
					{
						throw new FinanceNetException("cannot fetch Yahoo credentials");
					}
					return;
				}
				catch (Exception ex)
				{
					_sessionState.InvalidateSession();
					_logger.LogInformation($"Retry after exception={ex?.Message}");
					await Task.Delay(TimeSpan.FromSeconds(1 * attempt));
				}
			}
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task<string> CreateApiCookiesAndCrumb(CancellationToken token)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
		httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

		string? crumb = null;
		var response = await httpClient.GetAsync(Constants.YahooBaseUrlAuthentication.ToLower(), token).ConfigureAwait(false);

		var requestMessage = new HttpRequestMessage(HttpMethod.Get, Constants.YahooBaseUrlCrumbApi.ToLower());
		var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
		requestMessage.Headers.Add("Cookie", cookieHeader);

		response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
		crumb = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		if (string.IsNullOrEmpty(crumb) || crumb.Contains("Too Many Requests"))
		{
			throw new FinanceNetException("Unable to retrieve Yahoo crumb.");
		}

		if (_sessionState?.GetCookieContainer().Count < 3)
		{
			throw new FinanceNetException("Unable to get api cookies.");
		}
		var cookieString = _sessionState?.GetCookieContainer()?.GetCookies(new Uri(Constants.YahooBaseUrlHtml)).Cast<Cookie>().Select(cookie => cookie.Name);
		_logger.LogDebug(() => $"cookieNames= {cookieString}");
		_logger.LogDebug(() => $"_crumb= {crumb}");
		_logger.LogInformation($"API Session established successfully");
		return crumb;
	}

	private async Task CreateUiCookies(CancellationToken token)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
		httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

		// get consent
		await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
		var response = await httpClient.GetAsync(Constants.YahooBaseUrlHtml, token);
		response.EnsureSuccessStatusCode();

		var htmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlContent);
		var csrfTokenNode = document.QuerySelector("input[name='csrfToken']");
		var sessionIdNode = document.QuerySelector("input[name='sessionId']");
		if (csrfTokenNode == null || sessionIdNode == null)
		{
			response = await httpClient.GetAsync(Constants.YahooBaseUrlHtml, token);
			response.EnsureSuccessStatusCode();

			// no EU consent, call from coming outside of EU
			if (_sessionState?.GetCookieContainer()?.Count >= 3)
			{
				_logger.LogInformation($"UI Session established successfully without EU consent");
				return;
			}
			var cookieNames = string.Join(", ", _sessionState?.GetCookieContainer()?.GetCookies(new Uri(Constants.YahooBaseUrlHtml)).Cast<Cookie>().Select(cookie => cookie.Name));
			throw new FinanceNetException($"Unable to retrieve csrfTokenNode and sessionIdNode, cnt={_sessionState?.GetCookieContainer()?.Count},names={cookieNames}");
		}
		var csrfToken = csrfTokenNode.GetAttribute("value");
		var sessionId = sessionIdNode.GetAttribute("value");
		if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(sessionId))
		{
			var cookieNames = string.Join(", ", _sessionState?.GetCookieContainer()?.GetCookies(new Uri(Constants.YahooBaseUrlHtml)).Cast<Cookie>().Select(cookie => cookie.Name));
			throw new FinanceNetException($"Unable to retrieve csrfToken and sessionId, cnt={_sessionState?.GetCookieContainer()?.Count},names={cookieNames}");
		}
		await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

		// reject consent
		var postData = new List<KeyValuePair<string, string>>
						{
							new("csrfToken", csrfToken),
							new("sessionId", sessionId),
							new("originalDoneUrl", Constants.YahooBaseUrlHtml),
							new("namespace", "yahoo"),
						};
		foreach (var value in new List<string> { "reject", "reject" })
		{
			postData.Add(new("reject", value));
		}
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, (string?)$"{Constants.YahooBaseUrlConsentCollect}?sessionId={sessionId}")
		{
			Content = new FormUrlEncodedContent(postData)
		};
		requestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
		response = await httpClient.SendAsync(requestMessage, token);
		response.EnsureSuccessStatusCode();
		await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

		// finalize
		response = await httpClient.GetAsync(Constants.YahooBaseUrlHtml, token);
		response.EnsureSuccessStatusCode();
		if (_sessionState.GetCookieContainer()?.Count < 3)
		{
			var cookieNames = string.Join(", ", GetCookies().Select(cookie => cookie.Name));
			throw new FinanceNetException($"Unable to get ui cookies, cnt={_sessionState.GetCookieContainer()?.Count},names={cookieNames}");
		}
		if (_sessionState?.GetCookieContainer() != null && _sessionState?.GetCookieContainer()?.Count >= 3)
		{
			_logger.LogInformation($"UI Session established successfully");
		}
	}
}

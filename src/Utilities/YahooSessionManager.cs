﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Finance.Net.Utilities;

internal class YahooSessionManager(ILogger<IYahooSessionManager> logger,
		IHttpClientFactory httpClientFactory,
		IYahooSessionState sessionState,
		IReadOnlyPolicyRegistry<string> policyRegistry) : IYahooSessionManager
{
	private readonly ILogger<IYahooSessionManager> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
		return _sessionState.GetCookieContainer().GetCookies(new Uri(Constants.YahooBaseUrlHtml)).Cast<Cookie>();
	}

	public string GetCookieNames()
	{
		return string.Join(",", _sessionState.GetCookieContainer().GetCookies(new Uri(Constants.YahooBaseUrlHtml)).Cast<Cookie>().Select(e => e.Name));
	}

	public async Task RefreshSessionAsync(CancellationToken token = default)
	{
		_logger.LogDebug("cookieNames={cookies}", GetCookieNames());

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
			});
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
		httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

		var response = await httpClient.GetAsync(Constants.YahooBaseUrlAuthentication.ToLower(), token).ConfigureAwait(false);

		var requestMessage = new HttpRequestMessage(HttpMethod.Get, Constants.YahooBaseUrlCrumbApi.ToLower());
		var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
		requestMessage.Headers.Add("Cookie", cookieHeader);

		response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
		var crumb = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		if (string.IsNullOrEmpty(crumb) || crumb.Contains("Too Many Requests"))
		{
			throw new FinanceNetException("Unable to retrieve Yahoo crumb.");
		}

		if (_sessionState?.GetCookieContainer().Count < 3)
		{
			throw new FinanceNetException("Unable to get api cookies.");
		}
		_logger.LogDebug("cookieNames={cookies}", GetCookieNames());
		_logger.LogDebug("_crumb= {crumb}", crumb);
		_logger.LogInformation("API Session established successfully");
		return crumb;
	}

	private async Task CreateUiCookies(CancellationToken token)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
		httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");

		// get consent
		await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
		var response = await httpClient.GetAsync(Constants.YahooBaseUrlHtml, token);
		_logger.LogDebug("cookieNames={cookies}", GetCookieNames());
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
				_logger.LogInformation("UI Session established successfully without EU consent");
				return;
			}
			throw new FinanceNetException($"Unable to retrieve csrfTokenNode and sessionIdNode, cnt={_sessionState?.GetCookieContainer()?.Count}");
		}
		var csrfToken = csrfTokenNode.GetAttribute("value");
		var sessionId = sessionIdNode.GetAttribute("value");
		if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(sessionId))
		{
			throw new FinanceNetException($"Unable to retrieve csrfToken and sessionId, cnt={_sessionState?.GetCookieContainer()?.Count}");
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
		_logger.LogDebug("cookieNames={cookies}", GetCookieNames());
		response.EnsureSuccessStatusCode();
		await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

		// finalize
		response = await httpClient.GetAsync(Constants.YahooBaseUrlHtml, token);
		response.EnsureSuccessStatusCode();
		if (_sessionState.GetCookieContainer()?.Count < 3)
		{
			throw new FinanceNetException($"Unable to get ui cookies, cnt={_sessionState.GetCookieContainer()?.Count}");
		}
		if (_sessionState?.GetCookieContainer() != null && _sessionState?.GetCookieContainer()?.Count >= 3)
		{
			_logger.LogInformation("UI Session established successfully");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetFinance.Exceptions;
using NetFinance.Interfaces;

namespace NetFinance.Utilities;

internal class YahooSessionManager(ILogger<IYahooSessionManager> logger,
								   IOptions<NetFinanceConfiguration> options,
								   IYahooCredentialsManager yahooCredentialsManager) : IYahooSessionManager
{
	private readonly ILogger<IYahooSessionManager> _logger = logger;
	private readonly IYahooCredentialsManager _credentialsManager = yahooCredentialsManager ?? throw new ArgumentNullException(nameof(yahooCredentialsManager));
	private readonly NetFinanceConfiguration _options = options.Value ?? throw new ArgumentNullException(nameof(options));
	private static readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<(string userAgent, CookieCollection? cookies, string? crumb)> GetApiCredentials(CancellationToken token = default)
	{
		await RefreshSessionAsync(token).ConfigureAwait(false);
		return _credentialsManager.GetApiCredentials();
	}

	public async Task<(string userAgent, CookieCollection? cookies)> GetUiCredentials(CancellationToken token = default)
	{
		await RefreshSessionAsync(token).ConfigureAwait(false);
		return _credentialsManager.GetUiCredentials();
	}

	public string GetUiUserAgent()
	{
		return _credentialsManager.GetUiUserAgent();
	}

	public async Task RefreshSessionAsync(CancellationToken token = default, bool forceRefresh = false)
	{
		if (forceRefresh)
		{
			_credentialsManager.SetApiCredentials(null, null);
			_credentialsManager.SetUiCredentials(null);
		}
		if (_credentialsManager.AreValid())
		{
			return;
		}

		await _semaphore.WaitAsync(token).ConfigureAwait(false);

		if (_credentialsManager.AreValid())
		{
			return;
		}

		try
		{
			for (int attempt = 1; attempt <= 5; attempt++)
			{
				try
				{
					if (!_credentialsManager.AreApiCredentialsValid())
					{
						var (crumb, cookieContainer) = await CreateApiCookiesAndCrumb(token).ConfigureAwait(false);
						_credentialsManager.SetApiCredentials(crumb, cookieContainer);
					}
					if (!_credentialsManager.AreUiCredentialsValid())
					{
						var cookieContainer = await CreateUiCookiesAndCrumb(token).ConfigureAwait(false);
						_credentialsManager.SetUiCredentials(cookieContainer);
					}
				}
				catch (Exception ex)
				{
					_logger.LogInformation($"Retry after exception={ex?.Message}");
					await Task.Delay(TimeSpan.FromSeconds(2));
				}
			}
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task<(string? crumb, CookieContainer? cookieContainer)> CreateApiCookiesAndCrumb(CancellationToken token)
	{
		var userAgent = _credentialsManager.GetApiUserAgent();
		var handler = new HttpClientHandler
		{
			CookieContainer = new CookieContainer(),
			UseCookies = true
		};
		using (var httpClient = new HttpClient(handler))
		{
			httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
			httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
			httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

			string? crumb = null;
			var response = await httpClient.GetAsync(_options.Yahoo_BaseUrl_Authentication.ToLower(), token).ConfigureAwait(false);

			var requestMessage = new HttpRequestMessage(HttpMethod.Get, _options.Yahoo_BaseUrl_Crumb_Api.ToLower());
			var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
			requestMessage.Headers.Add("Cookie", cookieHeader);

			response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
			crumb = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			if (string.IsNullOrEmpty(crumb) || crumb.Contains("Too Many Requests"))
			{
				throw new NetFinanceException("Unable to retrieve Yahoo crumb.");
			}
			if (handler?.CookieContainer?.Count < 3)
			{
				throw new NetFinanceException("Unable to get api cookies.");
			}
			var cookieString = handler?.CookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html)).Cast<Cookie>().Select(cookie => cookie.Name);
			_logger.LogDebug(() => $"cookieNames= {cookieString}");
			_logger.LogDebug(() => $"_crumb= {crumb}");
			_logger.LogInformation($"API Session established successfully");
			return (crumb, handler?.CookieContainer);
		}
	}

	private async Task<CookieContainer> CreateUiCookiesAndCrumb(CancellationToken token)
	{
		var userAgent = _credentialsManager.GetUiUserAgent();
		var handler = new HttpClientHandler
		{
			CookieContainer = new CookieContainer(),
			UseCookies = true
		};
		using (var httpClient = new HttpClient(handler))
		{
			httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
			httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8");
			httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

			// get consent
			await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
			var response = await httpClient.GetAsync(_options.Yahoo_BaseUrl_Html, token);
			response.EnsureSuccessStatusCode();

			var htmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlContent);
			var csrfTokenNode = document.QuerySelector("input[name='csrfToken']");
			var sessionIdNode = document.QuerySelector("input[name='sessionId']");
			if (csrfTokenNode == null || sessionIdNode == null)
			{
				response = await httpClient.GetAsync(_options.Yahoo_BaseUrl_Html, token);
				response.EnsureSuccessStatusCode();

				// no EU consent, call from coming outside of EU
				if (handler?.CookieContainer?.Count >= 3)
				{
					_logger.LogInformation($"UI Session established successfully without EU consent");
					return handler.CookieContainer;
				}
				var cookieNames = string.Join(", ", handler?.CookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html)).Cast<Cookie>().Select(cookie => cookie.Name));
				throw new NetFinanceException($"Unable to retrieve csrfTokenNode and sessionIdNode, cnt={handler?.CookieContainer?.Count},names={cookieNames}");
			}
			var csrfToken = csrfTokenNode.GetAttribute("value");
			var sessionId = sessionIdNode.GetAttribute("value");
			if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(sessionId))
			{
				var cookieNames = string.Join(", ", handler?.CookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html)).Cast<Cookie>().Select(cookie => cookie.Name));
				throw new NetFinanceException($"Unable to retrieve csrfToken and sessionId, cnt={handler?.CookieContainer?.Count},names={cookieNames}");
			}
			await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

			// reject consent
			var postData = new List<KeyValuePair<string, string>>
						{
							new("csrfToken", csrfToken),
							new("sessionId", sessionId),
							new("originalDoneUrl", _options.Yahoo_BaseUrl_Html),
							new("namespace", "yahoo"),
						};
			foreach (var value in new List<string> { "reject", "reject" })
			{
				postData.Add(new("reject", value));
			}
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, (string?)$"{_options.Yahoo_BaseUrl_Consent_Collect}?sessionId={sessionId}")
			{
				Content = new FormUrlEncodedContent(postData)
			};
			requestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			response = await httpClient.SendAsync(requestMessage, token);
			response.EnsureSuccessStatusCode();
			await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);

			// finalize
			response = await httpClient.GetAsync(_options.Yahoo_BaseUrl_Html, token);
			response.EnsureSuccessStatusCode();
			if (handler.CookieContainer?.Count < 3)
			{
				var cookieNames = string.Join(", ", handler.CookieContainer.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html)).Cast<Cookie>().Select(cookie => cookie.Name));
				throw new NetFinanceException($"Unable to get ui cookies, cnt={handler.CookieContainer?.Count},names={cookieNames}");
			}
		};
		if (handler?.CookieContainer != null && handler?.CookieContainer?.Count >= 3)
		{
			_logger.LogInformation($"UI Session established successfully");
			return handler.CookieContainer;
		}
		return new();
	}
}

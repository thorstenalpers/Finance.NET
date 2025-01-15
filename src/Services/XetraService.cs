using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.XPath;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.Xetra;
using Finance.Net.Models.Xetra.Dto;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

/// <inheritdoc />
public class XetraService : IXetraService
{
    private readonly ILogger<XetraService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;
    private readonly AsyncPolicy _retryPolicy;

    /// <inheritdoc />
    public XetraService(ILogger<XetraService> logger,
                        IHttpClientFactory httpClientFactory,
                        IReadOnlyPolicyRegistry<string> policyRegistry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

        // do not use IoC, so users can use Automapper independently
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapMethod = m => false;
            cfg.AddProfile<XetraInstrumentAutomapperProfile>();
        });
        _mapper = config.CreateMapper();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Instrument>> GetInstrumentsAsync(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var url = await GetDownloadUrl(token).ConfigureAwait(false);
                var response = await httpClient.GetAsync(url, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";",
                };

                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                using var csv = new CsvReader(reader, config);
                await csv.ReadAsync();
                await csv.ReadAsync();
                csv.Context.RegisterClassMap<XetraInstrumentsMapping>();
                var records = csv.GetRecords<InstrumentItem>().ToList();

                var instruments = _mapper.Map<List<Instrument>>(records);
                var result = new List<Instrument>();
                foreach (var item in instruments)
                {
                    var mnemonic = item.Mnemonic;
                    if (string.IsNullOrWhiteSpace(mnemonic))
                    {
                        continue;
                    }
                    result.Add(item);
                }
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("Cannot fetch from Xetra", ex);
        }
    }

    private async Task<Uri?> GetDownloadUrl(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
        var url = Constants.XetraInstrumentsUrl.ToLowerInvariant();
        var baseUri = new Uri(url);

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var hrefAttributes = document.DocumentElement.SelectNodes("//a[contains(@class, 'download') and contains(., 'All tradable instruments')]/@href")?.Select(e => e.NodeValue);
                var relativeDownloadUrl = hrefAttributes?.FirstOrDefault();
                return new Uri(baseUri, relativeDownloadUrl);
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"Cannot fetch from {Constants.XetraInstrumentsUrl}", ex);
        }
    }
}

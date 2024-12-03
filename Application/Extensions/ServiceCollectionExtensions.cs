using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetFinance.Application.Options;
using NetFinance.Application.Services;
using NetFinance.Application.Utilities;

namespace NetFinance.Application.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Configures .NetFinanceService
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="configuration">Optional: Default values to configure NetFinanceService. <see cref="NetFinanceOptions"/> ></param>
	public static void AddNetFinance(this IServiceCollection services, IConfiguration? configuration = null)
	{
		var yahooFinanceSection = configuration?.GetSection(NetFinanceOptions.SectionName);

		if (yahooFinanceSection?.Exists() == true)
		{
			services.Configure<NetFinanceOptions>(yahooFinanceSection);
		}
		else
		{
			services.Configure<NetFinanceOptions>(_ => { });
		}
		services.AddSingleton<IYahooSession, YahooSession>();
		services.AddScoped<INetFinanceService, NetFinanceService>();

		services.AddHttpClient(Constants.ApiClientName, (serviceProvider, client) =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<NetFinanceOptions>>().Value;

			var userAgent = Helper.CreateRandomUserAgent();
			client.DefaultRequestHeaders.Add("User-Agent", userAgent);
			client.Timeout = TimeSpan.FromSeconds(options.Timeout);
		});
	}
}
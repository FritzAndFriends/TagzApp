using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TagzApp.Common.Exceptions;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Blazot.Configuration;
using TagzApp.Providers.Blazot.Converters;
using TagzApp.Providers.Blazot.Events;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Services;

namespace TagzApp.Providers.Blazot;

public class StartBlazot : IConfigureProvider
{
	private readonly ILogger<StartBlazot> _Logger;
	// TODO: Is this empty Constructor really needed? I don't see any references to it. Check CS8618: Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public StartBlazot() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public StartBlazot(ILogger<StartBlazot> logger)
	{
		_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection config;

		try
		{
			config = configuration.GetSection(BlazotClientConfiguration.AppSettingsSection);
			services.Configure<BlazotClientConfiguration>(config);
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Error configuring Blazot: {message}", ex.Message);
			throw new InvalidConfigurationException(ex.Message, BlazotClientConfiguration.AppSettingsSection);
		}

		var settings = config.Get<BlazotClientConfiguration>();

		if (string.IsNullOrWhiteSpace(settings?.BaseAddress?.ToString()) || string.IsNullOrWhiteSpace(settings.ApiKey))
			return services;

		services.AddSingleton(configuration);
		services.AddHttpClient<ISocialMediaProvider, BlazotProvider, BlazotClientConfiguration>(configuration, BlazotClientConfiguration.AppSettingsSection);
		services.AddTransient<ISocialMediaProvider, BlazotProvider>();
		services.AddSingleton<IContentConverter, ContentConverter>();
		services.AddSingleton<ITransmissionsService, HashtagTransmissionsService>();
		services.AddSingleton<IAuthService, AuthService>();
		services.AddSingleton<IAuthEvents, AuthEvents>();

		return services;
	}
}

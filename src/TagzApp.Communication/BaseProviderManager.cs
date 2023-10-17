using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TagzApp.Communication.Extensions;

namespace TagzApp.Communication;

public class BaseProviderManager
{
	private readonly IServiceCollection _Services;
	private readonly IConfiguration _Configuration;
	private readonly ILogger<BaseProviderManager> _Logger;
	protected readonly IProviderConfigurationRepository? _ProviderConfigurationRepository;

	public IEnumerable<ISocialMediaProvider> Providers { get; private set; }

	public BaseProviderManager(IConfiguration configuration, ILogger<BaseProviderManager> logger,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders,
		IProviderConfigurationRepository? providerConfigurationRepository)
	{
		_Services = new ServiceCollection();
		_Configuration = configuration;
		_Logger = logger;
		_ProviderConfigurationRepository = providerConfigurationRepository;
		Providers = socialMediaProviders != null && socialMediaProviders.Count() > 0
			? socialMediaProviders : new List<ISocialMediaProvider>();
	}

	public async Task InitProviders()
	{
		if (!Providers.Any())
		{
			await LoadConfigurationProviders();
		}
	}

	private async Task LoadConfigurationProviders()
	{
		List<IConfigureProvider> configProviders = new();
		var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		if (!string.IsNullOrWhiteSpace(path))
		{
			foreach (string dllPath in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
			{

				if (dllPath.Contains("Microsoft.") || dllPath.Contains("System.") || dllPath.Contains("AspNet.") || dllPath.Contains("Azure.")) continue;

				try
				{
					var assembly = Assembly.LoadFrom(dllPath);
					var providerAssemblies = assembly.GetTypes()
						.Where(t => typeof(IConfigureProvider).IsAssignableFrom(t) && !t.IsInterface);

					if (providerAssemblies.Any())
					{
						foreach (var provider in providerAssemblies)
						{
							var providerInstance = Activator.CreateInstance(provider, _ProviderConfigurationRepository) as IConfigureProvider;

							if (providerInstance != null)
							{
								configProviders.Add(providerInstance);
							}
						}
					}
				}
				catch (BadImageFormatException)
				{
					_Logger.LogWarning($"Skipping {dllPath} - not a .NET dll");
				}
				catch (Exception ex)
				{
					_Logger.LogWarning(ex, $"Skipping {dllPath} due to error");
				}
			}

			await ConfigureProviders(configProviders);
		}
	}

	private async Task ConfigureProviders(IEnumerable<IConfigureProvider> configurationProviders)
	{
		var socialMediaProviders = new List<ISocialMediaProvider>();

		foreach (var provider in configurationProviders)
		{
			if (provider is INeedConfiguration)
			{
				((INeedConfiguration)provider).SetConfiguration(_Configuration);
			}
			await provider.RegisterServices(_Services);
		}

		_Services.AddPolicies(_Configuration);
		_Services.AddSingleton<IConfiguration>(_Configuration);
		var sp = _Services.BuildServiceProvider();
		socialMediaProviders.AddRange(sp.GetServices<ISocialMediaProvider>());
		Providers = socialMediaProviders;
	}
}

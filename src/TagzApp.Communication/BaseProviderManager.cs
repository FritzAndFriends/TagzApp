using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TagzApp.Communication.Extensions;

namespace TagzApp.Communication;

public class BaseProviderManager
{
	private readonly IServiceCollection _Services;
	private readonly ILogger<BaseProviderManager> _Logger;

	public IEnumerable<ISocialMediaProvider> Providers { get; private set; }

	public BaseProviderManager(ILogger<BaseProviderManager> logger,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders)
	{
		_Services = new ServiceCollection();
		_Logger = logger;
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
							var providerInstance = Activator.CreateInstance(provider) as IConfigureProvider;

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
			await provider.RegisterServices(_Services);
		}

		// TEMP: Commented out to get working
		_Services.AddPolicies();
		var sp = _Services.BuildServiceProvider();
		socialMediaProviders.AddRange(sp.GetServices<ISocialMediaProvider>());
		Providers = socialMediaProviders;
	}
}

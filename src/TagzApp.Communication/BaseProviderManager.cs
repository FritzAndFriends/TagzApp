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
  public IEnumerable<ISocialMediaProvider> Providers { get; private set; }

  public BaseProviderManager(IConfiguration configuration, ILogger<BaseProviderManager> logger, 
    IEnumerable<ISocialMediaProvider>? socialMediaProviders)
  {
    _Services = new ServiceCollection();
    _Configuration = configuration;
    _Logger = logger;
    Providers = socialMediaProviders != null && socialMediaProviders.Count() > 0 
      ? socialMediaProviders : new List<ISocialMediaProvider>();
  }

  public void InitProviders()
  {
    if (!Providers.Any())
    {
      LoadConfigurationProviders();
    }
  }

  private void LoadConfigurationProviders()
  {
    List<IConfigureProvider> configProviders = new List<IConfigureProvider>();
    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    if (!string.IsNullOrWhiteSpace(path))
    {
      foreach (string dllPath in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
      {

				if (dllPath.Contains("Microsoft.") || dllPath.Contains("System.")) continue;

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
        } catch(Exception ex) {
          _Logger.LogWarning(ex, $"Skipping {dllPath} due to error");
        }
      }

      ConfigureProviders(configProviders);
    }
  }

  private void ConfigureProviders(IEnumerable<IConfigureProvider> configurationProviders)
  {
    var socialMediaProviders = new List<ISocialMediaProvider>();

    foreach (var provider in configurationProviders)
    {
      provider.RegisterServices(_Services, _Configuration);
    }

    _Services.AddPolicies(_Configuration);

    var sp = _Services.BuildServiceProvider();
    socialMediaProviders.AddRange(sp.GetServices<ISocialMediaProvider>());
    Providers = socialMediaProviders;
  }
} 

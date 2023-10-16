using System.Reflection;
using System.ComponentModel;
using TagzApp.Common.Attributes;

namespace TagzApp.Web.Services;

public class ViewModelUtilitiesService
{
	private readonly ILogger<ViewModelUtilitiesService> _Logger;

	public ViewModelUtilitiesService(ILogger<ViewModelUtilitiesService> logger)
	{
		_Logger = logger;
	}

	public PropertyInfo[]? LoadViewModel(string providerName)
	{
		var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		if (!string.IsNullOrWhiteSpace(path))
		{
			string dllPath = Directory.GetFiles(path, $"TagzApp.Providers.{providerName}.dll", SearchOption.AllDirectories).FirstOrDefault() ?? string.Empty;

			try
			{
				var assembly = Assembly.LoadFrom(dllPath);
				var viewModelAssembly = assembly.GetTypes()
					.FirstOrDefault(t => typeof(IProviderConfigurationViewModel).IsAssignableFrom(t) && !t.IsInterface);

				var properties = viewModelAssembly?.GetProperties();

				return properties;
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

		_Logger.LogWarning($"Unable to load view model for provider {providerName}!");
		throw new Exception($"Unable to load view model for provider {providerName}!");
	}

	public string GetDisplayName(PropertyInfo propertyInfo)
	{
		var displayNameAttribute = propertyInfo.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
		return displayNameAttribute?.DisplayName ?? string.Empty;
	}

	public string GetInputType(PropertyInfo propertyInfo)
	{
		var inputTypeAttribute = propertyInfo.GetCustomAttribute(typeof(InputTypeAttribute)) as InputTypeAttribute;
		return inputTypeAttribute?.InputType ?? string.Empty;
	}
}

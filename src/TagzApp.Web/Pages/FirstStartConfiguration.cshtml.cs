using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Web.Pages;

public class FirstStartConfigurationModel : PageModel
{
	private readonly IHostApplicationLifetime _ApplicationLifetime;

	public FirstStartConfigurationModel(IHostApplicationLifetime applicationLifetime)
	{
		_ApplicationLifetime = applicationLifetime;
	}

	[BindProperty]
	public string ConfigurationType { get; set; }

	[BindProperty, Required]
	public string Provider { get; set; }

	[BindProperty, Required]
	public string ConnectionString { get; set; }

	[BindProperty]
	public string ContentProvider { get; set; }

	[BindProperty]
	public string? ContentConnectionString { get; set; }

	[BindProperty]
	public string SecurityProvider { get; set; }

	[BindProperty]
	public string? SecurityConnectionString { get; set; }

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (ModelState.IsValid)
		{

			// Grab the ConfigureTagzAppFactory and set the values that were submitted
			await ConfigureTagzAppFactory.SetConfigurationProvider(Provider, ConnectionString);

			if (ConfigurationType.Equals("basic", StringComparison.InvariantCultureIgnoreCase))
			{

				SecurityProvider = Provider;
				ContentProvider = Provider;

				SecurityConnectionString = ConnectionString;
				ContentConnectionString = ConnectionString;

			}

			// Configure the Security and Content providers
			var currentProvider = ConfigureTagzAppFactory.Current;
			var connectionSettings = new ConnectionSettings
			{
				SecurityProvider = SecurityProvider,
				SecurityConnectionString = SecurityConnectionString,
				ContentProvider = ContentProvider,
				ContentConnectionString = ContentConnectionString
			};
			await currentProvider.SetConfigurationById(ConnectionSettings.ConfigurationKey, connectionSettings);

			Program.Restart();

			return RedirectToPage("/Index");
		}

		return Page();
	}


}

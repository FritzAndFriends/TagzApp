using Microsoft.AspNetCore.Components;

namespace TagzApp.Blazor.Components.Pages;

public partial class FirstStartConfiguration
{

	[Inject]
	public IHostApplicationLifetime _ApplicationLifetime { get; set; }

	[Inject]
	public NavigationManager? NavigationManager { get; set; }

	[SupplyParameterFromForm]
	public FirstStartConfig Config { get; set; }


}

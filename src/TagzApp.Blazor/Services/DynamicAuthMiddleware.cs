using Microsoft.Extensions.Primitives;

namespace TagzApp.Blazor.Services;

public class DynamicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DynamicAuthMiddleware> _logger;

    public DynamicAuthMiddleware(RequestDelegate next, ILogger<DynamicAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path.StartsWithSegments("/Account/PerformExternalLogin"))
		{
			var provider = context.Request.Form["provider"];
			var configService = context.RequestServices.GetRequiredService<IConfigureTagzApp>();

			// Check if this provider is properly configured
			var hasConfig = await CheckProviderConfiguration(configService, provider);
			if (!hasConfig)
			{
				context.Response.Redirect($"/Account/ProviderNotConfigured?provider={provider}");
				return;
			}
		}

		await _next(context);
	}

	public static async Task<bool> CheckProviderConfiguration(IConfigureTagzApp configService, string provider)
	{
		return (
			!string.IsNullOrEmpty(await configService.GetConfigurationStringById($"Authentication:{provider}:ClientID"))
			&& !string.IsNullOrEmpty(await configService.GetConfigurationStringById($"Authentication:{provider}:ClientSecret"))
			);

	}
}

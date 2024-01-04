namespace TagzApp.Blazor;

public class StartupConfigMiddleware(RequestDelegate next, IConfiguration configuration)
{

	public async Task Invoke(HttpContext context)
	{

		// Exit now if the app is already configured
		//if (!ConfigureTagzAppFactory.IsConfigured &&
		//	!context.Request.Path.Value!.StartsWith("/FirstStartConfiguration") &&
		//	!context.Request.Path.Value.EndsWith(".js") &&
		//	!context.Request.Path.Value.EndsWith(".css"))
		//{
		//	Console.WriteLine("Redirecting for first start");
		//	context.Response.Redirect("/FirstStartConfiguration");
		//}

		await next(context);

	}

}

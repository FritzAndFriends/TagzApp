using TagzApp.Providers.Mastodon;
namespace TagzApp.Web;


public class Program
{
	private static void Main(string[] args)
	{
		BuildServer(args).Run();
	}

	public static WebApplication BuildServer(string[] args)
	{

		var app = builder.Build();

		return BuildServer(args, null);

	}

	public static WebApplication BuildServer(string[] args, Action<IServiceCollection> serviceConfig)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorPages();

		if (serviceConfig is null)
		{

			builder.Services.AddTagzAppHostedServices();

		}
		else
		{

			serviceConfig(builder.Services);

		}

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();

		app.MapRazorPages();

		return app;

	}
}
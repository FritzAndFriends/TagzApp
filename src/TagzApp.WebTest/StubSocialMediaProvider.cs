using Bogus;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TagzApp.WebTest;

public class StubSocialMediaProvider : ISocialMediaProvider
{
	public string Id { get; } = "TEST";
	public string DisplayName { get; } = "TEST";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromMilliseconds(1000);

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		var testContent = new Faker<Content>()
				.RuleFor(f => f.Author, f => new Creator()
				{
					DisplayName = f.Person.FullName,
					ProfileImageUri = new Uri(f.Internet.Avatar()),
					ProfileUri = new Uri(f.Internet.Url()),
					UserName = f.Internet.UserName()
				})
				.RuleFor(f => f.HashtagSought, tag.Text.TrimStart('#').ToLowerInvariant())
				.RuleFor(f => f.Provider, f => "shield-fill-x")
				.RuleFor(f => f.ProviderId, Guid.NewGuid().ToString())
				.RuleFor(f => f.SourceUri, f => new Uri(f.Internet.Url()))
				.RuleFor(f => f.Text, f => f.Lorem.Lines(3) + $"<br/>{tag.Text}")
				.RuleFor(f => f.Timestamp, f => f.Date.Recent())
			;

		return Task.FromResult(testContent.Generate(10).AsEnumerable());
	}

	public Task StartAsync()
	{
		return Task.CompletedTask;
	}
}

public static class StubSocialMediaProviderExtensions
{
	public static IServiceCollection UseOnlyStubSocialMediaProvider(this IServiceCollection services)
	{
		services.RemoveAll<ISocialMediaProvider>();
		services.AddSingleton<ISocialMediaProvider, StubSocialMediaProvider>();
		return services;
	}

	public static IHostBuilder UseOnlyStubSocialMediaProvider(this IHostBuilder builder) =>
		builder.ConfigureServices(services => services.UseOnlyStubSocialMediaProvider());

	public static IWebHostBuilder UseOnlyStubSocialMediaProvider(this IWebHostBuilder builder) =>
		builder.ConfigureServices(services => services.UseOnlyStubSocialMediaProvider());
}

public class StartStubSocialMediaProvider : IConfigureProvider
{
	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton<ISocialMediaProvider, StubSocialMediaProvider>();

		return services;
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		services.AddSingleton<ISocialMediaProvider, StubSocialMediaProvider>();

		return await Task.FromResult(services);
	}
}

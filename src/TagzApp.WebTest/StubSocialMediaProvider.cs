using Bogus;

namespace TagzApp.WebTest;

public class StubSocialMediaProvider : ISocialMediaProvider
{
	public string Id { get; } = "TEST";
	public string DisplayName { get; } = "TEST";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromMilliseconds(1000);

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var testContent = new Faker<Content>()
			.RuleFor(f => f.Author, f => new Creator() {
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
			.RuleFor(f => f.Timestamp, f=> f.Date.Recent())
			;

		return Task.FromResult(testContent.Generate(10).AsEnumerable());


	}
}

public class StartStubSocialMediaProvider : IConfigureProvider
{

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		
		services.AddSingleton<ISocialMediaProvider, StubSocialMediaProvider>();

		return services;

	}

}

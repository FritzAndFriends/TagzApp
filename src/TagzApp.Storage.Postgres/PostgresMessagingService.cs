using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagzApp.Communication;
using TagzApp.Web.Services;

namespace TagzApp.Storage.Postgres;

public class PostgresMessagingService : BaseProviderManager, IMessagingService
{

	private readonly IServiceProvider _Services;
	private readonly INotifyNewMessages _NotifyNewMessages;

	public PostgresMessagingService(
		IServiceProvider services,
		INotifyNewMessages notifyNewMessages,
		IConfiguration configuration,
		ILogger<BaseProviderManager> logger,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders) : base(configuration, logger, socialMediaProviders)
	{
		_Services = services;
		_NotifyNewMessages = notifyNewMessages;
	}

	private List<string> _TagsTracked = new();
	private PostgresMessaging _Service;

	public IEnumerable<string> TagsTracked => _TagsTracked;

	public async Task AddHashtagToWatch(string tag)
	{

		// Persist in memory
		tag = Hashtag.ClearFormatting(tag);
		_TagsTracked.Add(tag);

		// Add to the database
		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();
		ctx.TagsWatched.Add(new Tag { Text = tag });
		await ctx.SaveChangesAsync();

		_Service.SubscribeToContent(new Hashtag() { Text = tag },
			c =>
			{
				_NotifyNewMessages.Notify(Hashtag.ClearFormatting(c.HashtagSought), c);
			});


	}

	public async Task<Content?> GetContentByIds(string provider, string providerId)
	{

		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		var foundContent = await ctx.Content.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Provider == provider && c.ProviderId == providerId);

		return foundContent is null ? null : (Content)foundContent;

	}

	public async Task<IEnumerable<Content>> GetExistingContentForTag(string tag)
	{

		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();
		return (await ctx.Content.OrderByDescending(c => c.Timestamp)
																				.Take(50)
																				.ToListAsync())
																				.Select(c => (Content)c).ToArray();

	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{

		// Get existing tags from database
		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();
		_TagsTracked.AddRange((await ctx.TagsWatched.ToArrayAsync()).Select(t => t.Text));

		InitProviders();
		_Service = new PostgresMessaging(_Services);
		_Service.StartProviders(Providers, cancellationToken);

		foreach (var tag in _TagsTracked)
		{
			_Service.SubscribeToContent(new Hashtag() { Text = tag },
				c =>
				{
					_NotifyNewMessages.Notify(Hashtag.ClearFormatting(c.HashtagSought), c);
				});
		}

	}

	public Task StopAsync(CancellationToken cancellationToken)
	{

		_Service.Dispose();
		return Task.CompletedTask;

	}
}
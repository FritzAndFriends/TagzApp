using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagzApp.Common.Models;
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
				_NotifyNewMessages.NotifyNewContent(Hashtag.ClearFormatting(c.HashtagSought), c);
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
					_NotifyNewMessages.NotifyNewContent(Hashtag.ClearFormatting(c.HashtagSought), c);
				});
		}

	}

	public Task StopAsync(CancellationToken cancellationToken)
	{

		_Service.Dispose();
		return Task.CompletedTask;

	}

	public async Task<IEnumerable<Content>> GetApprovedContentByTag(string tag)
	{

		tag = $"#{tag}";

		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();
		var outRecords = await ctx.Content.AsNoTracking()
			.Include(c => c.ModerationAction)
			.Where(c => c.HashtagSought == tag && 
					c.ModerationAction != null && 
					c.ModerationAction.State == ModerationState.Approved)
			.OrderByDescending(c => c.Timestamp)
			.Take(50)
			.Select(c => (Content)c)
			.ToListAsync();

		return outRecords;

	}

	public async Task<IEnumerable<(Content, ModerationAction?)>> GetContentByTagForModeration(string tag)
	{

		tag = $"#{tag}";

		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();
		var contentResults = await ctx.Content.AsNoTracking()
			.Include(c => c.ModerationAction)
			.Where(c => c.HashtagSought == tag)
			.OrderByDescending(c => c.Timestamp)
			.Take(100)
			.ToListAsync();

		var outResults = new List<(Content, ModerationAction?)> ();
		foreach (var c in contentResults)
		{

			outResults.Add(((Content)c, c.ModerationAction is null ? null : (ModerationAction)c.ModerationAction));

		}

		return outResults;

	}

}

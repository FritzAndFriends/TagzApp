using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
	// TODO: Check if _services actually can be null. The compiler is complaining about it.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public PostgresMessagingService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		IServiceProvider services,
		INotifyNewMessages notifyNewMessages,
		IMemoryCache cache,
		IConfiguration configuration,
		ILogger<BaseProviderManager> logger,
		ILogger<AzureSafetyModeration> azureSafetyLogger,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders,
		IProviderConfigurationRepository providerConfigurationRepository) :
		base(configuration, logger, socialMediaProviders, providerConfigurationRepository)
	{
		_Services = services;
		_NotifyNewMessages = new AzureSafetyModeration(cache, notifyNewMessages, services, configuration, azureSafetyLogger);
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

		await InitProviders();
		_Service = new PostgresMessaging(_Services, _ProviderConfigurationRepository!);
		await _Service.StartProviders(Providers, cancellationToken);

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

		var outResults = new List<(Content, ModerationAction?)>();
		foreach (var c in contentResults)
		{

			outResults.Add(((Content)c, c.ModerationAction is null ? null : (ModerationAction)c.ModerationAction));

		}

		return outResults;

	}

	public string GetLatestProviderIdByTagAndProvider(string tag, string provider)
	{

		tag = $"#{tag}";

		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		return ctx.Content.AsNoTracking()
			.Where(c => c.HashtagSought == tag && c.Provider == provider)
			.OrderByDescending(c => c.Timestamp)
			.Select(c => c.ProviderId)
			.FirstOrDefault() ?? string.Empty;

	}

	public async Task<IEnumerable<(Content, ModerationAction?)>> GetFilteredContentByTag(string tag, string[] providers, ModerationState[] states)
	{

		tag = $"#{tag}";

		using var scope = _Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		if (states.Length == 1 && states.Contains(ModerationState.Pending))
		{

			return (await ctx.Content.AsNoTracking()
				.Include(c => c.ModerationAction)
				.Where(c => c.HashtagSought == tag &&
					providers.Contains(c.Provider) &&
					c.ModerationAction == null)
				.OrderByDescending(c => c.Timestamp)
				.Take(100)
				.ToArrayAsync())
				.Select(c => ((Content)c, (ModerationAction?)null))
				.ToArray();

		}
		else if (states.Length == 3)
		{

			return (await ctx.Content.AsNoTracking()
				.Include(c => c.ModerationAction)
				.Where(c => c.HashtagSought == tag &&
					providers.Contains(c.Provider))
				.OrderByDescending(c => c.Timestamp)
				.Take(100)
				.ToArrayAsync())
				.Select(c => ((Content)c, c.ModerationAction == null ? null : (ModerationAction?)c.ModerationAction))
				.ToArray();

		}
		else
		{

			return (await ctx.Content.AsNoTracking()
				.Include(c => c.ModerationAction)
				.Where(c => c.HashtagSought == tag &&
					providers.Contains(c.Provider) &&
					c.ModerationAction != null &&
					states.Contains(c.ModerationAction.State))
				.OrderByDescending(c => c.Timestamp)
				.Take(100)
				.ToArrayAsync())
				.Select(c => ((Content)c, (ModerationAction?)(c.ModerationAction)))
				.ToArray();

		}

	}
}

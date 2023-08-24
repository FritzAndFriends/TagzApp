using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using TagzApp.Storage.Postgres;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;
using TagzApp.Web.Services.Base;

namespace TagzApp.Web.Services;

public class PostgresMessagingService : BaseProviderManager, IHostedService
{

	private IContentSubscriber _ContentSubscriber;
	private IContentPublisher _ContentPublisher;
	private IModerationPublisher _ModerationPublisher;
	private IModerationSubscriber _ModerationSubscriber;
	private readonly PgSqlContext _Context;
	private List<Task> _ProviderTasks = new List<Task>();
	private readonly IHubContext<MessageHub> _HubContext;
	private readonly ILogger<PostgresMessagingService> _Logger;


	public PostgresMessagingService(
		IConfiguration configuration,
		IHubContext<MessageHub> hubContext,
		ILogger<PostgresMessagingService> logger,
		PgSqlContentPublisher contentPublisher,
		PgSqlContentSubscriber contentSubscriber,
		PgModerationPublisher moderationPublisher,
		PgModerationSubscriber moderationSubscriber,
		PgSqlContext context,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders = null
	) : base(configuration, logger, socialMediaProviders)
	{
		_HubContext = hubContext;
		_Logger = logger;

		_ContentPublisher = contentPublisher;
		_ContentSubscriber = contentSubscriber;
		_ModerationPublisher = moderationPublisher;
		_ModerationSubscriber = moderationSubscriber;
		_Context = context;
	}

	/// <summary>
	/// A collection of the tags and the content found for them.
	/// </summary>
	public readonly Dictionary<string, ConcurrentBag<Content>> Content = new Dictionary<string, ConcurrentBag<Content>>();

	#region Hosted Service Implementation

	public Task StartAsync(CancellationToken cancellationToken)
	{
		InitProviders();
		StartProviders(_Providers, cancellationToken);

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{

		return Task.CompletedTask;
	}

	public void StartProviders(IEnumerable<ISocialMediaProvider> providers, CancellationToken cancellationToken)
	{

		_ProviderTasks.Clear();
		foreach (var providerItem in providers)
		{

			_ProviderTasks.Add(Task.Factory.StartNew(async (object state) =>
			{

				var provider = (ISocialMediaProvider)state;

				var lastQueryTime = DateTimeOffset.UtcNow.AddHours(-1);

				while (!cancellationToken.IsCancellationRequested)
				{

					if (!_ContentSubscriber.HasSubscribers)
					{
						await Task.Delay(TimeSpan.FromSeconds(1));
						continue;
					}

					foreach (var tag in _ContentSubscriber.TagsSubscribedTo)
					{

						var formattedTag = Hashtag.ClearFormatting(tag);

						Hashtag thisTag = new Hashtag() { Text = tag };
						var contentIdentified = await provider.GetContentForHashtag(thisTag, lastQueryTime);
						lastQueryTime = DateTime.UtcNow;

						foreach (var item in contentIdentified.OrderBy<Content, DateTimeOffset>(c => c.Timestamp))
						{
								await _ContentPublisher.PublishContentAsync(thisTag, item);
						}

						await Task.Delay(provider.NewContentRetrievalFrequency);

					}

				}

			}, providerItem));

		}

	}


	#endregion

	public Task AddHashtagToWatch(string tag)
	{

		_ContentSubscriber.SubscribeToContentAsync(new Hashtag() { Text = tag },
			c => {
				Content[c.HashtagSought.TrimStart('#')].Add(c);
				_HubContext.Clients
					.Group(c.HashtagSought.TrimStart('#').ToLowerInvariant())
					.SendAsync("NewMessage", (ContentModel)c);
				//_Logger.LogInformation($"Message found for tag '{c.HashtagSought}': {c.Text}");
			});

		return Task.CompletedTask;

	}

	public IEnumerable<Content> GetExistingContentForTag(string tag)
	{

		var content = _Context.Contents
			.Where(c => c.HashtagSought == Hashtag.ClearFormatting(tag))
			.OrderByDescending(c => c.Timestamp)
			.Take(200)
			.ToArray();

		return content;

	}

}
﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace TagzApp.Storage.Postgres;

internal class PostgresMessaging : IDisposable
{
	private bool _DisposedValue;

	private CancellationTokenSource _CancellationTokenSource;
	private List<Task> _ProviderTasks = new List<Task>();
	internal readonly Dictionary<string, ConcurrentQueue<Content>> Queue = new();
	private readonly Dictionary<string, ConcurrentBag<Action<Content>>> _Actions = new();
	private static IServiceProvider _Services;


	public PostgresMessaging(IServiceProvider services)
	{
		_Services = _Services ?? services;
	}

	internal void StartProviders(IEnumerable<ISocialMediaProvider> providers, CancellationToken cancellationToken)
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

					if (!_Actions.Any())
					{
						await Task.Delay(TimeSpan.FromSeconds(1));
						continue;
					}

					using var scope = _Services.CreateScope();
					var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

					foreach (var tag in _Actions.Keys.Distinct<string>())
					{

						Hashtag thisTag = new Hashtag() { Text = tag };
						var contentIdentified = await provider.GetContentForHashtag(thisTag, lastQueryTime);
						var providerIds = contentIdentified.Select(c => c.ProviderId).Distinct().ToArray();
						lastQueryTime = DateTime.UtcNow;

						// de-dupe with in-database collection
						var inDb = await context.Content.AsNoTracking()
							.Where(c => c.Provider == provider.Id && providerIds.Any(i => i == c.ProviderId))
							.Select(c => c.ProviderId)
							.ToArrayAsync();
						contentIdentified = contentIdentified
							.ExceptBy(inDb, c => c.ProviderId)
							.ToArray();

						if (contentIdentified.Any())
						{

							context.Content.AddRange(contentIdentified.Select(c => (PgContent)c).ToArray());
							await context.SaveChangesAsync();

							foreach (var item in contentIdentified.OrderBy<Content, DateTimeOffset>(c => c.Timestamp))
							{
								await PublishContentAsync(thisTag, item);
							}

						}

					}

					await Task.Delay(provider.NewContentRetrievalFrequency);

				}

			}, providerItem));

		}

	}

	public Task PublishContentAsync(Hashtag tag, Content newContent)
	{

		if (!Queue.ContainsKey(tag.Text))
		{
			Queue.Add(tag.Text, new ConcurrentQueue<Content>());
		}

		Queue[tag.Text].Enqueue(newContent);

		return Task.CompletedTask;

	}


	internal void SubscribeToContent(Hashtag tag, Action<Content> onNewContent)
	{

		if (!_Actions.ContainsKey(Hashtag.ClearFormatting(tag.Text)))
		{
			_Actions.Add(Hashtag.ClearFormatting(tag.Text), new ConcurrentBag<Action<Content>>());
		}

		_Actions[Hashtag.ClearFormatting(tag.Text)].Add(onNewContent);

	}

	#region Dispose Pattern Stuff

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~PostgresMessaging()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion

}
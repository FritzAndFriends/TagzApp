using Gravatar;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using TagzApp.ViewModels.Data;

namespace TagzApp.Blazor.Hubs;

public class ModerationService
{

	private readonly ConcurrentDictionary<string, string> _CurrentUsersModerating = new();
	private readonly ConcurrentDictionary<string, DateTimeOffset> _CurrentUsersHeartbeating = new();
	private readonly IServiceProvider _Services;
	private readonly Timer _HeartbeatCleanup;


	public event EventHandler<ModeratorArgs> OnNewModerator;

	public event EventHandler<ModeratorArgs> OnDepartModerator;

	public event EventHandler<NewContentArgs> OnNewContent;

	public event EventHandler<ModeratedContentArgs> OnModeratedContent;

	public event EventHandler<BlockedUserCountArgs> OnBlockedUserCountChanged;

	public ModerationService(IServiceProvider services)
	{
		_Services = services;
		_HeartbeatCleanup = new Timer(HeartbeatCleanup, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
	}

	public NewModerator[] Moderators => _CurrentUsersModerating.Select(kvp => new NewModerator(kvp.Key, kvp.Key.ToGravatar(), kvp.Value)).ToArray();

	public void AddModerator(string email, string displayName)
	{

		var added = _CurrentUsersModerating.TryAdd(email, displayName);
		Console.WriteLine($"Added moderator {displayName} {(added ? "successfully" : "already exists")}");
		if (added) OnNewModerator?.Invoke(this, new ModeratorArgs { DisplayName = displayName, Email = email, Avatar = new Uri(email.ToGravatar()) });

	}

	public void RemoveModerator(string email)
	{

		_CurrentUsersModerating.TryRemove(email, out var displayName);
		OnDepartModerator?.Invoke(this, new ModeratorArgs { DisplayName = displayName, Email = email });

	}

	public int BlockedUserCount { get; private set; }

	public void SetBlockedUserCount(int count)
	{

		BlockedUserCount = count;
		OnBlockedUserCountChanged?.Invoke(this, new BlockedUserCountArgs { BlockedUserCount = count });

	}	

	public void NewContent(Content content) {

		OnNewContent?.Invoke(this, new NewContentArgs { Content = (ContentModel)content });

	}

	public void ModerateContent(Content content, ModerationAction action)
	{

		OnModeratedContent?.Invoke(this, new ModeratedContentArgs { Content = ModerationContentModel.ToModerationContentModel(content, action) });

	}

	public void Heartbeat(string displayName)
	{
		Console.WriteLine($"Heartbeat: {displayName}");
		_CurrentUsersHeartbeating[displayName] = DateTimeOffset.UtcNow;
	}

	private void HeartbeatCleanup(object? state)
	{

		// If a moderator hasn't sent a heartbeat in 5 seconds, remove them
		var now = DateTimeOffset.UtcNow;
		foreach (var kvp in _CurrentUsersHeartbeating)
		{
			Console.WriteLine($"Checking if its time to evict {kvp.Key} {kvp.Value} - {now}");
			if (now - kvp.Value > TimeSpan.FromSeconds(5))
			{
				Console.WriteLine($"Removing moderator without heartbeat: {kvp.Key}");
				_CurrentUsersHeartbeating.TryRemove(kvp.Key, out _);
				_CurrentUsersModerating.TryRemove(kvp.Key, out string displayName);
				OnDepartModerator?.Invoke(this, new ModeratorArgs { DisplayName = displayName, Email = kvp.Key });
			}
		}

	}


}

public class ModeratedContentArgs : EventArgs
{

	public required ModerationContentModel Content { get; set; }

}

public class BlockedUserCountArgs : EventArgs
{

	public required int BlockedUserCount { get; init; }

}

public class ModeratorArgs : EventArgs
{

	public string DisplayName { get; set; }

	public string Email {  get; set; }

	public Uri Avatar { get; set; }

}

public class NewContentArgs : EventArgs
{

	public required ContentModel Content { get; init; }

}

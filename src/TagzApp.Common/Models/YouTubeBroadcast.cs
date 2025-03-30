using System.Text.Json.Serialization;

namespace TagzApp.Common.Models;

[JsonSerializable(typeof(YouTubeBroadcast))]
public record YouTubeBroadcast(string Id, string Title, DateTimeOffset? BroadcastTime, string LiveChatId);


using System.ComponentModel.DataAnnotations;

namespace TagzApp.Common.Models;

public class ModerationAction
{

	public long Id { get; set; }

	public required string Provider { get; set; }

	public required string ProviderId { get; set; }

	public ModerationState PreviousState { get; set; } = ModerationState.Pending;

	public required ModerationState State { get; set; } = ModerationState.Pending;

	[MaxLength(100)]
	public string? Reason { get; set; }

	[MaxLength(100)]
	public string? Moderator { get; set; }

	public required DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

}

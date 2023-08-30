using System.ComponentModel.DataAnnotations;

namespace TagzApp.Storage.Postgres;

internal class PgModerationAction
{

	public long Id { get; set; }

	[MaxLength(20)]
	public required string Provider { get; set; }

	[MaxLength(50)]
	public required string ProviderId { get; set; }

	public required ModerationState State { get; set; } = ModerationState.Pending;

	[MaxLength(100)]
	public string? Reason { get; set; }

	[MaxLength(100)]
	public string? Moderator { get; set; }

	public required DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

	public PgContent? Content { get; set; }

	public long ContentId { get; internal set; }

	public static explicit operator PgModerationAction(ModerationAction action)
	{

		return new PgModerationAction
		{
			Id = action.Id,
			Provider = action.Provider,
			ProviderId = action.ProviderId,
			State = action.State,
			Reason = action.Reason,
			Moderator = action.Moderator,
			Timestamp = action.Timestamp
		};


	}

	public static explicit operator ModerationAction(PgModerationAction action)
	{

		return new ModerationAction
		{
			Id = action.Id,
			Provider = action.Provider,
			ProviderId = action.ProviderId,
			State = action.State,
			Reason = action.Reason,
			Moderator = action.Moderator,
			Timestamp = action.Timestamp
		};

	}


}

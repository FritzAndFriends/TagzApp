using System.ComponentModel.DataAnnotations.Schema;

namespace TagzApp.Storage.Postgres;

public class ModerationAction
{

	public string Provider { get; set; }
	public string ProviderId { get; set; }

	public ModerationState State { get; set; }

	public string UserId { get; set; }

	public DateTimeOffset Timestamp { get; set; }

	public Content Content { get; set; }

}
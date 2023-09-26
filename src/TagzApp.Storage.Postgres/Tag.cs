using System.ComponentModel.DataAnnotations;

namespace TagzApp.Storage.Postgres;

public class Tag
{

	[Key]
	public required string Text { get; set; }

}

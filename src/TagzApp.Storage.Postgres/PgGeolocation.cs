using System.ComponentModel.DataAnnotations;

namespace TagzApp.Storage.Postgres;

public class PgGeolocation
{

	[Key]
	public required string Name { get; set; }

	public required decimal Latitude { get; set; }

	public required decimal Longitude { get; set; }

}

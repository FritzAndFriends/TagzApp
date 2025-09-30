using Fritz.Charlie.Common;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Storage.Postgres;

public class PgViewerLocation
{

	[MaxLength(16)]
	public required string StreamId { get; set; }

	[MaxLength(32)]
	public required string HashedUserId { get; set; }

	[MaxLength(100)]
	public required string Description { get; set; }

	public required decimal Latitude { get; set; }

	public required decimal Longitude { get; set; }

	public static explicit operator ViewerLocationEvent(PgViewerLocation value)
	{
		return new ViewerLocationEvent(value.Latitude, value.Longitude, value.Description)
		{
			StreamId = value.StreamId,
			UserId = value.HashedUserId
		};
	}

	public static explicit operator PgViewerLocation(ViewerLocationEvent value)
	{
		return new PgViewerLocation
		{
			StreamId = value.StreamId,
			HashedUserId = value.UserId,
			Description = value.LocationDescription,
			Latitude = value.Latitude,
			Longitude = value.Longitude
		};
	}

}

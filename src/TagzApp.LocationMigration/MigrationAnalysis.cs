namespace TagzApp.LocationMigration;

public class MigrationAnalysis
{
	public int TwitchRecords { get; set; }
	public int YouTubeRecords { get; set; }
	public int TotalRecords { get; set; }
	public HashSet<string> UniqueLocations { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	public int UsersToProcess { get; set; }
	public int ExistingUsers { get; set; }
	public int LocationsToGeocode { get; set; }
	public int CachedLocations { get; set; }
}

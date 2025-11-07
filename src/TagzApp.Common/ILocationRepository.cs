namespace TagzApp.Common;

public interface ILocationRepository
{

	Task<(string Description, decimal Latitude, decimal Longitude)> GetLocationFromTable(string location);

	Task AddLocationToTable(string description, decimal latitude, decimal longitude);

}

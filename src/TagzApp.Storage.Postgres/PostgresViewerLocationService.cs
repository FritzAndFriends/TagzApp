using Fritz.Charlie.Common;
using Fritz.Charlie.Components.Map;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Storage.Postgres;

public class PostgresViewerLocationService(IServiceScopeFactory serviceScopeFactory) : IViewerLocationService, ILocationRepository
{
	public event EventHandler<ViewerLocationEvent>? LocationPlotted;
	public event EventHandler<Guid>? LocationRemoved;

	public async Task AddLocationToTable(string description, decimal latitude, decimal longitude)
	{

		// add location to the locations table
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		context.Locations.Add(new PgGeolocation
		{
			Name = description.ToLowerInvariant(),
			Latitude = latitude,
			Longitude = longitude
		});
		await context.SaveChangesAsync();

	}

	public async Task ClearLocationsForStreamAsync(string streamId)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		// remove all PgViewerLocations with the given streamId
		await context.ViewerLocations.Where(v => v.StreamId == streamId).ExecuteDeleteAsync();
	}

	public async Task<IReadOnlyList<string>> GetAllStreamIdsAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		// return the distinct streamIds from PgViewerLocations
		return await context.ViewerLocations
				.Select(v => v.StreamId)
				.Distinct()
				.OrderBy(v => v)
				.ToListAsync();
	}

	public async Task<(string Description, decimal Latitude, decimal Longitude)> GetLocationFromTable(string location)
	{

		location = location.Trim().ToLowerInvariant();

		// get location from the locations table
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();
		var loc = await context.Locations
			.AsNoTracking()
			.Where(l => l.Name == location)
			.FirstOrDefaultAsync();

		if (loc == null)
		{
			return ("", 0m, 0m);
		}

		return (loc.Name, loc.Latitude, loc.Longitude);

	}

	public async Task<IReadOnlyList<ViewerLocationEvent>> GetLocationsForStreamAsync(string streamId)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		var locations = await context.ViewerLocations
				.Where(v => v.StreamId == streamId)
				.ToListAsync();

		return locations.Select(l => (ViewerLocationEvent)l)
				.ToList();
	}

	public async Task PlotLocationAsync(ViewerLocationEvent locationEvent)
	{
		await SaveLocationAsync(locationEvent);

		// raise the event for the map to plot
		LocationPlotted?.Invoke(this, locationEvent);
	}

	public async Task RemoveLocationAsync(string streamId, string userId)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		// remove location from the database
		await context.ViewerLocations.Where(v => v.StreamId == streamId && v.HashedUserId == userId)
				.ExecuteDeleteAsync();
	}

	public async Task SaveLocationAsync(ViewerLocationEvent locationEvent)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

		// write the location to the database
		var entity = (PgViewerLocation)locationEvent;
		context.ViewerLocations.Add(entity);
		await context.SaveChangesAsync();
	}
}

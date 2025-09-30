using Fritz.Charlie.Common;
using Fritz.Charlie.Components.Map;
using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

public class PostgresViewerLocationService(TagzAppContext context) : IViewerLocationService
{
	public event EventHandler<ViewerLocationEvent>? LocationPlotted;
	public event EventHandler<Guid>? LocationRemoved;

	public async Task ClearLocationsForStreamAsync(string streamId)
	{

		// remove all PgViewerLocations with the given streamId
		await context.ViewerLocations.Where(v => v.StreamId == streamId).ExecuteDeleteAsync();

	}

	public async Task<IReadOnlyList<string>> GetAllStreamIdsAsync()
	{
		// return the distinct streamIds from PgViewerLocations
		return await context.ViewerLocations
			.Select(v => v.StreamId)
			.Distinct()
			.OrderBy(v => v)
			.ToListAsync();

	}

	public async Task<IReadOnlyList<ViewerLocationEvent>> GetLocationsForStreamAsync(string streamId)
	{

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

		// remove location from the database
		await context.ViewerLocations.Where(v  => v.StreamId == streamId && v.HashedUserId == userId)
			.ExecuteDeleteAsync();

	}

	public async Task SaveLocationAsync(ViewerLocationEvent locationEvent)
	{

		// write the location to the database
		var entity = (PgViewerLocation)locationEvent;
		context.ViewerLocations.Add(entity);
		await context.SaveChangesAsync();

	}
}

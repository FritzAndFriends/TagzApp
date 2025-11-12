# TagzApp Location Migration Tool

This console application migrates historical location data from chat content in the TagzApp database. It extracts location information from Twitch and YouTube-Chat messages and geocodes them using the Nominatim API with intelligent caching.

## Features

- **Intelligent Caching**: Loads existing geocoded locations from the database to avoid redundant API calls
- **Smart Location Validation**: Filters out non-location text like technical terms, chat expressions, and programming code
- **Case-Insensitive Matching**: Location lookups are case-insensitive (e.g., "Paris", "PARIS", "paris" are treated as the same location)
- **Rate Limiting**: Respects Nominatim API limits with configurable delays between requests
- **Performance Metrics**: Tracks cache hits, API calls, and processing statistics
- **Dry-Run Analysis**: Preview what the migration will do without making any changes
- **Comprehensive Logging**: Detailed logging for monitoring and troubleshooting

## Purpose

The map feature was added to TagzApp to display viewer locations mentioned in Twitch and YouTube chat messages. This migration tool processes historical chat messages to extract and geocode location mentions that were received before the location extraction feature was active.

## What It Does

1. **Scans Content Table**: Reads all content from `TWITCH` and `YOUTUBE-CHAT` providers
2. **Extracts Locations**: Uses the Fritz.Charlie LocationTextService to identify location mentions in chat messages
3. **Caches Known Locations**: Loads existing geocoded locations from the Locations table to reduce API calls
4. **Geocodes New Locations**: Uses OpenStreetMap API to convert location names to coordinates
5. **Saves to Database**: Stores the first location found for each user in the ViewerLocations table
6. **Avoids Duplicates**: Skips users who already have locations in the ViewerLocations table

## Location Validation

The tool includes intelligent filtering to avoid geocoding non-location text that the LocationTextService might extract. Common false positives that are filtered out include:

### Technical Terms
- Programming keywords: `dotnet`, `code`, `debug`, `build`, `async`, `await`
- File extensions: `.cs`, `.js`, `.html`, `.json`
- Commands: `npm install`, `git commit`, `docker run`

### Chat Expressions
- Common phrases: `same`, `lol`, `thanks`, `hello`
- Pronouns and contractions: `we're`, `it's`, `that's`, `i'm`
- Questions: `what is`, `who is`, `can you`

### Programming Code
- Operators: `==`, `!=`, `&&`, `||`, `=>`
- Method calls: `function method`, `class interface`
- Variables: Strings with excessive numbers or symbols

### Valid Location Examples
The validation allows legitimate locations like:
- **Cities**: Paris, Tokyo, New York, San Francisco
- **Countries/States**: Canada, Germany, California, Texas  
- **Geographic features**: North Carolina, South Beach, Mountain View
- **Formal names**: Saint Paul, New Orleans, Little Rock

When the dry-run analysis encounters invalid location text, it logs messages like:
```
[DRY-RUN] Skipping invalid location text: 'no dotnet-dump' from user TWITCH-username
[DRY-RUN] Skipping invalid location text: 'we're getting set' from user TWITCH-username
```

## Setup

1. **Update Connection String**: Edit `appsettings.json` and replace the DefaultConnection with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=tagzapp;Username=postgres;Password=yourpassword"
  }
}
```

2. **Build the Project**:
```bash
cd src/TagzApp.LocationMigration
dotnet build
```

## Usage

### Running the Migration

Run the migration tool:

```bash
cd src/TagzApp.LocationMigration
dotnet run
```

This will:
1. Load existing geocoded locations into memory cache
2. Query Content table for Twitch and YouTube-Chat records
3. Extract location information from chat content
4. Geocode new locations using Nominatim API (with caching)
5. Save new ViewerLocation records to the database

### Dry-Run Analysis

To analyze what the migration would do without making any changes:

```bash
dotnet run --dry-run
# or
dotnet run -d
```

The dry-run will:
- Count Twitch and YouTube-Chat records in the Content table
- Analyze content for location patterns
- Report how many unique locations would be geocoded
- Show cache hit statistics from existing Locations table
- **No database modifications or API calls are made**

## Example Output

### Regular Migration
```
info: Starting TagzApp Location Migration Tool
info: Loading location cache from database...
info: Cache loaded: 1,245 locations in 0.8 seconds
info: Processing content records...
info: Processed 15,000 records in 45.2 seconds
info: Cache hits: 890, API calls: 355
info: Migration completed successfully
```

### Dry-Run Analysis
```
info: Starting TagzApp Location Migration Tool (DRY RUN)
info: Loading existing viewer locations to avoid duplicates...

=== DRY RUN ANALYSIS RESULTS ===
Content Analysis:
  - Total Twitch records: 8,245
  - Total YouTube-Chat records: 2,156
  - Total records to process: 10,401

Location Analysis:
  - Unique locations found in content: 1,234
  - Locations already cached: 889 (72.0%)
  - New locations requiring geocoding: 345 (28.0%)

Cache Statistics:
  - Existing cached locations: 1,245
  - Expected cache hit rate: 72.0%
  - Estimated Nominatim API calls: 345

No changes were made to the database.
```

Or with a specific connection string:

```bash
dotnet run -- --ConnectionStrings:DefaultConnection="Host=prod-server;Database=tagzapp;Username=user;Password=pass"
```

## Performance Features

### Location Caching
The tool includes intelligent caching to minimize API calls:
- **Database Cache**: Loads all existing geocoded locations from the `Locations` table at startup
- **In-Memory Lookup**: Checks cache first before making API calls to Nominatim
- **Auto-Save**: New geocoded locations are automatically saved to the cache table for future use
- **Duplicate Prevention**: Avoids re-geocoding the same location text multiple times

This caching system significantly reduces the number of API requests needed, especially when processing messages that mention common locations multiple times.

## Output

The tool provides detailed logging showing:
- Location cache loading progress
- Total number of Content records found
- Progress updates every 100 processed records
- Cache hits vs. API calls for geocoding
- Location extraction and geocoding results
- Final summary of locations saved

Example output:
```
[12:34:56 INF] Starting TagzApp Location Migration Tool
[12:34:56 INF] Found 15423 content records from Twitch and YouTube Chat to process
[12:35:02 INF] Processed 100/15423 records. Found 3 locations, saved 3.
[12:35:15 DBG] Found location 'Seattle, WA' in message from user TWITCH-StreamerName
[12:35:16 INF] Saved location for user TWITCH-StreamerName: Seattle, WA at (47.6062, -122.3321)
[12:40:23 INF] Migration completed. Processed 15423 records, found 87 locations, saved 73 unique user locations.
```

## Technical Details

### Location Detection
- Uses Fritz.Charlie.Components.Services.ILocationTextService to detect location patterns
- Recognizes various formats: city names, "City, State", "City, Country", etc.
- Only processes the first location found per user to avoid spam

### Geocoding
- Uses OpenStreetMap Nominatim API for geocoding
- Includes rate limiting to respect API limits (1 request per second)
- Validates coordinates and skips invalid/delayed results
- Caches results to avoid duplicate API calls

### User Identification
- Creates consistent user IDs: `"{Provider}-{DisplayName}"`
- Fixes the existing issue where YouTube users had empty usernames
- Uses DisplayName instead of UserName for consistency

### Database Schema
Saves to ViewerLocations table:
- **StreamId**: Always "all" for global map display
- **HashedUserId**: Provider + DisplayName (not actually hashed despite the name)
- **Description**: The location text as found in the message
- **Latitude/Longitude**: Decimal coordinates from geocoding

## Limitations

- Only processes Twitch (`TWITCH`) and YouTube Chat (`YOUTUBE-CHAT`) content
- Only saves the first location found for each user
- Depends on OpenStreetMap API availability and rate limits
- Location text detection quality depends on Fritz.Charlie service patterns
- May miss location mentions that don't match expected patterns

## Error Handling

The tool includes comprehensive error handling:
- Continues processing if individual records fail
- Logs warnings for geocoding failures
- Graceful handling of malformed JSON data
- Database transaction safety

## Performance

- Processes records in batches with progress reporting
- Uses async enumeration for memory efficiency
- Includes rate limiting for external API calls
- Typical processing: ~100-200 records per minute (depending on location frequency)

## After Migration

Once the migration completes:
1. New locations will automatically be processed in real-time by the main application
2. The map at `/Map` will show both migrated historical data and new live locations
3. The migration tool can be run again safely - it will skip existing user locations

## Troubleshooting

**Connection Issues**: Verify the connection string and network access to the database

**No Locations Found**: Check if Content table has records with Provider = 'TWITCH' or 'YOUTUBE-CHAT'

**Geocoding Failures**: OpenStreetMap API may be rate-limited or unavailable. The tool will continue and can be re-run later.

**Permission Errors**: Ensure the database user has INSERT permissions on ViewerLocations table
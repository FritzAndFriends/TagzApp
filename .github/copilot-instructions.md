# TagzApp - Social Media Hashtag Aggregator

TagzApp is a .NET 9.0 Blazor Server application that aggregates hashtag content from multiple social media platforms including Twitter/X, Mastodon, Bluesky, YouTube, TwitchChat, and more. The application uses .NET Aspire for local orchestration and supports both PostgreSQL and SQLite databases.

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites and Installation
- **CRITICAL**: Install .NET 9.0.200 SDK exactly as specified in `global.json`
  ```bash
  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --version 9.0.200
  export PATH="/home/runner/.dotnet:$PATH"
  ```
- Git repository must be unshallowed for version calculations: `git fetch --unshallow`
- Docker (optional, for PostgreSQL container via scripts)

### Build and Test Process
**NEVER CANCEL any build or test commands. All timeouts listed include safety margins.**

1. **Restore Dependencies** (50-60 seconds, timeout: 90+ seconds)
   ```bash
   cd src
   dotnet restore
   ```
   - **NEVER CANCEL**: Takes ~51 seconds. Set timeout to 90+ seconds minimum.

2. **Build Main Application** (10-15 seconds after restore, timeout: 30+ seconds)
   ```bash
   cd src
   dotnet build TagzApp.Blazor --no-restore
   ```
   - **NEVER CANCEL**: Takes ~11 seconds after restore. Set timeout to 30+ seconds.
   - Expect 79+ warnings (nullable reference warnings, unused fields) - these are known and do not affect functionality.

3. **Release Build** (10-15 seconds, timeout: 30+ seconds)
   ```bash
   cd src
   dotnet build TagzApp.Blazor --configuration Release --no-restore
   ```

4. **Run Unit Tests** (3-5 seconds, timeout: 15+ seconds)
   ```bash
   cd src/TagzApp.UnitTest
   dotnet test --verbosity normal
   ```
   - **NEVER CANCEL**: Takes ~3.6 seconds. Set timeout to 15+ seconds.
   - **EXPECTED**: 2 test failures in Mastodon provider tests due to external API dependencies - this is normal.
   - **SUCCESS CRITERIA**: 22 total tests, ~18 pass, ~2 fail, ~2 skip.

### Code Quality and Formatting
**ALWAYS run formatting before committing or CI will fail.**

1. **Check Code Formatting** (30-40 seconds, timeout: 60+ seconds)
   ```bash
   cd src
   dotnet format --verbosity normal --exclude **/Migrations/ --verify-no-changes
   ```
   - **NEVER CANCEL**: Takes ~35 seconds. Set timeout to 60+ seconds.
   - Will show formatting errors that must be fixed.

2. **Fix Code Formatting** (30-40 seconds, timeout: 60+ seconds)
   ```bash
   cd src
   dotnet format --verbosity normal --exclude **/Migrations/
   ```
   - **ALWAYS RUN** before committing changes.

### Running the Application

#### Option 1: Using .NET Aspire (Recommended for full development)
```bash
cd src
dotnet run --project TagzApp.AppHost
```
- **NOTE**: Requires Docker for PostgreSQL and Redis containers.
- **LIMITATION**: May fail in containerized environments due to Kubernetes orchestration requirements.
- **ALTERNATIVE**: Use direct Blazor run if Aspire fails.

#### Option 2: Direct Blazor Application (Fallback)
```bash
cd src/TagzApp.Blazor
dotnet run
```
- **USE WHEN**: Aspire orchestration fails or Docker unavailable.
- **LIMITATION**: Requires manual database setup and configuration.

#### Option 3: Using Docker Compose
```bash
docker-compose up
```
- Uses pre-built container image from GitHub Container Registry.

### Database Management Scripts
Located in `scripts/` directory:
- `launchdb` / `launchdb.cmd`: Start PostgreSQL container
- `AddMigration.cmd`: Add new Entity Framework migration
- `UpdateDatabase.cmd`: Apply database migrations
- `RemoveMigration.cmd`: Remove last migration

## Validation and Testing

### Manual Validation Requirements
**ALWAYS perform manual testing after making changes to UI components.**

1. **Basic Application Flow**:
   - Navigate to homepage
   - Attempt to search for a hashtag (may fail without proper configuration - expected)
   - Check that the UI loads without errors
   - Verify moderation panel accessibility (requires authentication)

2. **After UI Changes**:
   - Test the waterfall display (main content view)
   - Test the moderation interface if applicable
   - Verify overlay display functionality
   - Check responsive design on different screen sizes

3. **Authentication Flow** (if providers configured):
   - Test login/logout functionality
   - Verify admin panel access with proper roles

### Pre-Commit Validation Checklist
**ALWAYS complete this checklist before committing:**

1. **Code Quality**:
   ```bash
   cd src
   dotnet format --verbosity normal --exclude **/Migrations/
   dotnet build TagzApp.Blazor --no-restore
   cd TagzApp.UnitTest
   dotnet test --verbosity normal
   ```

2. **Expected Results**:
   - Build succeeds with warnings (acceptable)
   - Unit tests: ~18 pass, ~2 fail (Mastodon), ~2 skip (YouTube)
   - No new formatting violations introduced

## Key Project Structure

### Primary Projects
- **TagzApp.Blazor**: Main server-side Blazor application
- **TagzApp.Blazor.Client**: Client-side Blazor components (WebAssembly)
- **TagzApp.AppHost**: .NET Aspire orchestration host
- **TagzApp.Common**: Shared models and utilities
- **TagzApp.Communication**: SignalR hubs and messaging

### Provider Projects (Social Media Integrations)
- **TagzApp.Providers.Twitter**: Twitter/X integration
- **TagzApp.Providers.Mastodon**: Mastodon API integration  
- **TagzApp.Providers.Bluesky**: Bluesky AT Protocol integration
- **TagzApp.Providers.Youtube**: YouTube video and chat integration
- **TagzApp.Providers.TwitchChat**: Twitch chat integration
- **TagzApp.Providers.Blazot**: Blazot platform integration
- **TagzApp.Providers.AzureQueue**: Azure Storage Queue integration

### Data Projects
- **TagzApp.Storage.Postgres**: PostgreSQL Entity Framework implementation
- **TagzApp.Storage.Postgres.Security**: PostgreSQL security and identity
- **TagzApp.Storage.Sqlite.Security**: SQLite security and identity
- **TagzApp.Security**: Authentication and authorization

### Test Projects
- **TagzApp.UnitTest**: xUnit unit tests
- **TagzApp.WebTest**: Playwright end-to-end tests

## Configuration and Environment

### Required Environment Variables
- Database connection strings (PostgreSQL or SQLite)
- Social media API keys and secrets (optional for basic functionality)
- TwitchRelayUri for Twitch integration
- Azure Storage connection strings for queue integration

### Configuration Files
- `.env`: Development environment variables
- `.env.local`: Local overrides (gitignored)
- `appsettings.json`: Application configuration
- `global.json`: .NET SDK version specification

## Build Timing Expectations

**All times include safety margins. NEVER CANCEL based on these times.**

| Operation | Expected Time | Timeout Setting |
|-----------|---------------|-----------------|
| dotnet restore | ~51 seconds | 90+ seconds |
| dotnet build | ~11 seconds | 30+ seconds |
| dotnet test | ~3.6 seconds | 15+ seconds |
| dotnet format | ~35 seconds | 60+ seconds |

## Common Issues and Solutions

### GitVersioning Errors
**Error**: "Shallow clone lacks the objects required to calculate version height"
**Solution**: `git fetch --unshallow`

### Build Warnings
**Expected**: 79+ compiler warnings related to nullable references and unused fields. These do not affect functionality and are acceptable.

### Test Failures
**Expected**: Mastodon provider tests fail due to external API dependencies. YouTube tests skip due to missing API keys.

### Aspire Orchestration Issues
**Symptoms**: Kubernetes/DCP connection errors
**Solution**: Fall back to direct Blazor application run: `dotnet run --project TagzApp.Blazor`

### Formatting Failures
**Error**: CI formatting check fails
**Solution**: Always run `dotnet format` before committing changes

## CI/CD Pipeline

### GitHub Actions Workflows
- **dotnet.yml**: Main build, test, and Docker image creation
- **formatting.yml**: Automated code formatting on non-main branches
- **TagzAppTwitchRelay.yml**: Twitch relay service deployment

### Build Requirements
- .NET 9.0 SDK (specified in global.json)
- All tests must pass (except known Mastodon failures)
- Code formatting must be clean
- No breaking changes to public APIs

## Developer Notes

### Navigation and Key Files
- **Main Application**: `src/TagzApp.Blazor/Program.cs`
- **Routing**: `src/TagzApp.Blazor/Components/App.razor`
- **SignalR Hubs**: `src/TagzApp.Blazor/Hubs/`
- **Provider Interfaces**: `src/TagzApp.Common/Models/`
- **Database Context**: `src/TagzApp.Storage.Postgres/`

### When Making Changes
1. **Always** run build and tests before committing
2. **Always** run `dotnet format` to fix code style
3. **Always** test UI changes manually in a browser
4. **Never** commit code that breaks the build
5. **Never** ignore test failures unless they match known external dependency failures

### Performance Considerations
- Build times are optimized with NuGet package caching
- Restore only needs to run once unless dependencies change
- Use `--no-restore` flag for subsequent builds
- Database operations may be slow on first run due to migrations

This application serves as a real-time social media aggregation tool with extensive provider support and modern .NET development practices.
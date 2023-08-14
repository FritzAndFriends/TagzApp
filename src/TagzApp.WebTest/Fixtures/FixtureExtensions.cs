namespace TagzApp.WebTest.Fixtures;

public static class FixtureExtensions
{
  public static IHostBuilder UseUniqueDb(this IHostBuilder builder, Guid id) =>
    builder.ConfigureAppConfiguration(configuration =>
    {
      var testConfiguration = new Dictionary<string, string?>()
      {
        {"ConnectionStrings:SecurityContextConnection",$"Data Source=TagzApp.Web.{id:N}.db" }
      };
      configuration.AddInMemoryCollection(testConfiguration);
    });

  public static async Task CleanUpDbFilesAsync(this Guid id, ILogger? logger = null)
  {
    logger ??= Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    // The host should have shutdown here so we can delete the test database files
    await Task.Delay(50);
    var dbFiles = System.IO.Directory.GetFiles(".", $"TagzApp.Web.{id:N}.db*");
    foreach (var dbFile in dbFiles)
    {
      try
      {
        logger.LogInformation("Removing test database file {File}", dbFile);
        System.IO.File.Delete(dbFile);
      }
      catch (Exception e)
      {
        logger.LogWarning("Could not remove test database file {File}: {Reason}", dbFile, e.Message);
      }
    }
  }
}

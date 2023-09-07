// Ignore Spelling: Css

using System.ComponentModel.DataAnnotations;

namespace TagzApp.Web.Data;

public class ApplicationConfiguration
{

	[Required, MaxLength(30)]
	public string SiteName { get; set; } = "TagzApp";

	[Required]
	public string WaterfallHeaderMarkdown { get; set; } = "# Welcome to TagzApp";

	public string WaterfallHeaderCss { get; set; } = string.Empty;

}

public class EntityConfigurationSource : IConfigurationSource
{
	private readonly string? _connectionString;

	public EntityConfigurationSource(string? connectionString) =>
			_connectionString = connectionString;

	public IConfigurationProvider Build(IConfigurationBuilder builder) =>
			new EntityConfigurationProvider(_connectionString);
}

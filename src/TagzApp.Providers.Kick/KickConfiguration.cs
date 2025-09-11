using System.Text.Json.Serialization;
using TagzApp.Common;

namespace TagzApp.Providers.Kick;

public class KickConfiguration : BaseProviderConfiguration<KickConfiguration>
{
	/// <summary>
	/// The configuration key used to store this configuration in the TagzApp configuration system
	/// </summary>
	protected override string ConfigurationKey => "provider-kick";

	[JsonPropertyOrder(1)]
	public string ChannelName { get; set; } = string.Empty;

	[JsonPropertyOrder(2)]
	public string ApiKey { get; set; } = string.Empty;

	public static KickConfiguration Empty => new()
	{
		ChannelName = string.Empty,
		ApiKey = string.Empty
	};

	[JsonIgnore]
	public override string Name => "Kick";

	[JsonIgnore]
	public override string Description => "Read all messages from a specified Kick channel";
	
	public override bool Enabled { get; set; }

	[JsonIgnore]
	public override string[] Keys => ["ChannelName", "ApiKey"];

	public override string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ChannelName" => ChannelName,
			"ApiKey" => ApiKey,
			"Enabled" => Enabled.ToString(),
			_ => string.Empty
		};
	}

	public override void SetConfigurationByKey(string key, string value)
	{
		switch (key)
		{
			case "ChannelName":
				ChannelName = value;
				break;
			case "ApiKey":
				ApiKey = value;
				break;
			case "Enabled":
				Enabled = bool.Parse(value);
				break;
			default:
				throw new NotImplementedException($"Unable to set value for key '{key}'");
		}
	}

	/// <summary>
	/// Updates this instance with values from another configuration instance
	/// </summary>
	/// <param name="source">The source configuration to copy from</param>
	protected override void UpdateFromConfiguration(KickConfiguration source)
	{
		ChannelName = source.ChannelName;
		ApiKey = source.ApiKey;
		Enabled = source.Enabled;
	}

	/// <summary>
	/// Public method to update configuration from another instance
	/// </summary>
	/// <param name="source">The source configuration to copy from</param>
	public void UpdateFrom(KickConfiguration source)
	{
		UpdateFromConfiguration(source);
	}

	/// <summary>
	/// Gets the configuration key used by this configuration type
	/// </summary>
	internal new static string GetConfigurationKey() => "provider-kick";
}
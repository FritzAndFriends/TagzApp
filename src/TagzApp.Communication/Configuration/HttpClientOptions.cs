using System.Text.Json;
using System.Text.Json.Serialization;
using TagzApp.Common.Attributes;

namespace TagzApp.Communication.Configuration;

/// <summary>
/// Declaration of Http Client options
/// </summary>
public class HttpClientOptions
{
	/// <summary>
	/// Gets or sets the base address
	/// </summary>
	[InputType("Url"), JsonPropertyName("BaseAddress"), JsonPropertyOrder(100)]
	public Uri? BaseAddress { get; set; }

	/// <summary>
	/// Gets or sets the method timeout
	/// </summary>
	[InputType("Timeout"), JsonPropertyName("Timeout"), JsonPropertyOrder(101)]
	public TimeSpan Timeout { get; set; }

	/// <summary>
	/// Gets or sets the set of headers that will be added to all requests
	/// </summary>
	[JsonPropertyOrder(102)]
	public Dictionary<string, string>? DefaultHeaders { get; set; }

	/// <summary>
	/// Gets or sets the http2 version for all requests
	/// The default value is TRUE.
	/// </summary>
	[JsonPropertyOrder(103)]
	public bool UseHttp2 { get; set; } = true;

	public static Dictionary<string, string> DeserializeHeaders(string json)
	{
		if (string.IsNullOrEmpty(json)) return new();
		return JsonSerializer.Deserialize<Dictionary<string, string>>(json);

	}

}


public static class HttpClientOptionsExtensions
{

	public static string Serialize(this Dictionary<string, string>? headers)
	{
		if (headers is null) return string.Empty;
		return JsonSerializer.Serialize(headers);

	}

}

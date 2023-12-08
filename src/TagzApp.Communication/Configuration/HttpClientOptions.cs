using System.Runtime.CompilerServices;
using System.Text.Json;
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
	[InputType("Uri")]
	public Uri? BaseAddress { get; set; }

	/// <summary>
	/// Gets or sets the method timeout
	/// </summary>
	[InputType("Timeout")]
	public TimeSpan Timeout { get; set; }

	/// <summary>
	/// Gets or sets the set of headers that will be added to all requests
	/// </summary>
	public Dictionary<string, string>? DefaultHeaders { get; set; }

	/// <summary>
	/// Gets or sets the http2 version for all requests
	/// The default value is TRUE.
	/// </summary>
	public bool UseHttp2 { get; set; } = true;

	public static Dictionary<string,string> DeserializeHeaders(string json)
	{
		if (string.IsNullOrEmpty(json)) return new();
		return JsonSerializer.Deserialize<Dictionary<string, string>>(json);

	}

}


public static class HttpClientOptionsExtensions
{

	public static string Serialize(this Dictionary<string,string>? headers)
	{
		if (headers is null) return string.Empty;
		return JsonSerializer.Serialize(headers);

	}

}

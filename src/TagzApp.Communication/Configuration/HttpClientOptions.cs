namespace TagzApp.Communication.Configuration;

/// <summary>
/// Declaration of Http Client options
/// </summary>
public class HttpClientOptions
{
	/// <summary>
	/// Gets or sets the base address
	/// </summary>
	public Uri? BaseAddress { get; set; }

	/// <summary>
	/// Gets or sets the method timeout
	/// </summary>
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
}

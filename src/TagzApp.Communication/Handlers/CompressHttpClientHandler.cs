using System.Net;

namespace TagzApp.Communication.Handlers;

/// <summary>
/// Declaration of a Http Client handler that ensures compression is enabled
/// </summary>
internal class CompressHttpClientHandler : HttpClientHandler
{
	/// <summary>
	/// Initializes a new instance of the CompressHttpClentHandler class
	/// </summary>
	/// <remarks>
	/// The default constructor initializes any fields to their default values.
	/// </remarks>
	public CompressHttpClientHandler() => AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.Deflate | DecompressionMethods.GZip;
}

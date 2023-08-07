namespace TagzApp.Communication.Configuration;

/// <summary>
/// Declaration of Retry Policy options
/// </summary>
internal class RetryPolicyOptions
{
	/// <summary>
	/// Gets or sets the count for retries
	/// The default value is 3
	/// </summary>
	public int Count { get; set; } = 3;

	/// <summary>
	/// Gets or sets the back-off power
	/// The default value is 2
	/// </summary>
	public int BackoffPower { get; set; } = 2;
}

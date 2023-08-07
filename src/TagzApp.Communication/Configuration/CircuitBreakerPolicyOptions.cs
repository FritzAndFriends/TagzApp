namespace TagzApp.Communication.Configuration;

/// <summary>
/// Declaration of Circuit Breaker options
/// </summary>
internal class CircuitBreakerPolicyOptions
{
	/// <summary>
	/// Gets or sets the break duration.
	/// The default value is 30 seconds
	/// </summary>
	public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Gets or sets the exceptions limit before a break is actioned.
	/// The default value is 12.
	/// </summary>
	public int ExceptionsAllowedBeforeBreaking { get; set; } = 12;
}

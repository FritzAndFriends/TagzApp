namespace TagzApp.Communication.Configuration;

/// <summary>
/// Declaration of Policy options
/// </summary>
internal class PolicyOptions
{
	/// <summary>
	/// Gets or set the Circuit Breaker policy options
	/// </summary>
	public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; } = new CircuitBreakerPolicyOptions();

	/// <summary>
	/// Gets or set the Retry policy options
	/// </summary>
	public RetryPolicyOptions HttpRetry { get; set; } = new RetryPolicyOptions();
}

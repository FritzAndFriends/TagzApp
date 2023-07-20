namespace TagzApp.Communication.Configuration;

/// <summary>
/// Constants for policies
/// </summary>
static internal class PolicyConstants
{
	/// <summary>
	/// Section name for HttpPolicies
	/// </summary>
	public const string HttpPolicies = nameof(HttpPolicies);

	/// <summary>
	/// Name of Circuit Breaker policy
	/// </summary>
	public const string HttpCircuitBreaker = nameof(HttpCircuitBreaker);

	/// <summary>
	/// Name of Retry policy
	/// </summary>
	public const string HttpRetry = nameof(HttpRetry);
}

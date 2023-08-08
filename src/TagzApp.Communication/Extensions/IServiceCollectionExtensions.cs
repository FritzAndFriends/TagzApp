using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System.Net;
using TagzApp.Communication.Configuration;
using TagzApp.Communication.Handlers;

namespace TagzApp.Communication.Extensions;

/// <summary>
/// Extensions to <see cref="IServiceCollection"/> to support content in the assembly
/// </summary>
public static class IServiceCollectionExtensions
{
	/// <summary>
	/// Adds the HTTP client.
	/// </summary>
	/// <typeparam name="TImplementation">The implementation of the interface.</typeparam>
	/// <typeparam name="TClientOptions">The type of the client options.</typeparam>
	/// <param name="services"><see cref="IServiceCollection"/> reference to add context to</param>
	/// <param name="configuration">Reference to the application configuration instance</param>
	/// <param name="configurationSectionName">Section name to use when configuring the Http client</param>
	/// <exception cref="ArgumentNullException">Raised whenever any of the provided arguments is null</exception>
	public static IServiceCollection AddHttpClient<TClient, TImplementation, TClientOptions>(this IServiceCollection services, IConfiguration configuration, string configurationSectionName)
					where TClient : class
					where TImplementation : class, TClient
					where TClientOptions : HttpClientOptions, new()
	{
		// Validate the request
		Ensure.Any.IsNotNull(services, nameof(services));
		Ensure.Any.IsNotNull(configuration, nameof(configuration));
		Ensure.String.IsNotNullOrEmpty(configurationSectionName, nameof(configurationSectionName));

		// Get the client builder with the configuration applied and return it
		return GetHttpClientBuild<TClient, TImplementation, TClientOptions>(services, configuration, configurationSectionName)
				.Services;
	}

	/// <summary>
	/// Gets the HTTP client build.
	/// </summary>
	/// <typeparam name="TClient">The type of the client.</typeparam>
	/// <typeparam name="TImplementation">The type of the implementation.</typeparam>
	/// <typeparam name="TClientOptions">The type of the client options.</typeparam>
	/// <param name="services"><see cref="IServiceCollection"/> reference to add context to</param>
	/// <param name="configuration">Reference to the application configuration instance</param>
	/// <param name="configurationSectionName">Section name to use when configuring the Http client</param>
	/// <exception cref="ArgumentNullException">Raised whenever any of the provided arguments is null</exception>
	private static IHttpClientBuilder GetHttpClientBuild<TClient, TImplementation, TClientOptions>(this IServiceCollection services, IConfiguration configuration, string configurationSectionName)
			where TClient : class
					where TImplementation : class, TClient
					where TClientOptions : HttpClientOptions, new()
	{
		// Validate the request
		Ensure.Any.IsNotNull(services, nameof(services));
		Ensure.Any.IsNotNull(configuration, nameof(configuration));
		Ensure.String.IsNotNullOrEmpty(configurationSectionName, nameof(configurationSectionName));

		// Return the http client build configured
		return services
			 .Configure<TClientOptions>(configuration.GetSection(configurationSectionName))
			 .AddHttpClient(typeof(TImplementation).Name)
			 .ConfigureHttpClient(
					 (serviceProvider, httpClient) =>
					 {
						 // Retrieve the configuration from the service
						 TClientOptions httpClientOptions = serviceProvider
												 .GetRequiredService<IOptions<TClientOptions>>()
												 .Value;

						 // Validate the base address value and set up the base address for the HTTP request if provided
						 httpClient.BaseAddress = Ensure.Any.IsNotNull(httpClientOptions.BaseAddress, nameof(httpClientOptions.BaseAddress));

						 // If additional parameters provided, then set them up
						 if (httpClientOptions.Timeout != TimeSpan.Zero)
						 {
							 httpClient.Timeout = httpClientOptions.Timeout;
						 }

						 if (httpClientOptions.DefaultHeaders?.Keys != null)
						 {
							 foreach (string headerName in httpClientOptions.DefaultHeaders.Keys)
							 {
								 httpClient.DefaultRequestHeaders.Add(headerName, httpClientOptions.DefaultHeaders[headerName]);
							 }
						 }

						 if (httpClientOptions.UseHttp2)
						 {
							 httpClient.DefaultRequestVersion = HttpVersion.Version20;
						 }
					 })

			 // Add the message handler and policies
			 .ConfigurePrimaryHttpMessageHandler(_ => new CompressHttpClientHandler())
			 .AddPolicyHandlerFromRegistry(PolicyConstants.HttpRetry)
			 .AddPolicyHandlerFromRegistry(PolicyConstants.HttpCircuitBreaker);
	}

	/// <summary>
	/// Extension method for an application to register Http policies with configuration
	/// </summary>
	/// <param name="services"><see cref="IServiceCollection"/> reference to add context to</param>
	/// <param name="configuration">Reference to the application configuration instance</param>
	/// <param name="configurationSectionName">Section name to use when configuring the Http client</param>
	/// <exception cref="ArgumentNullException">Raised whenever any of the provided arguments is null</exception>
	public static IServiceCollection AddPolicies(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = PolicyConstants.HttpPolicies)
	{
		// Validate the request
		Ensure.Any.IsNotNull(services, nameof(services));
		Ensure.Any.IsNotNull(configuration, nameof(configuration));
		Ensure.String.IsNotEmptyOrWhiteSpace(configurationSectionName, nameof(configurationSectionName));

		// Retrieve the policy options from service
		services.Configure<PolicyOptions>(configuration);
		PolicyOptions policyOptions = configuration.GetSection(configurationSectionName).Get<PolicyOptions>()!;

		IPolicyRegistry<string> policyRegistry = services.AddPolicyRegistry();

		// Add retry policy
		policyRegistry.Add(
				PolicyConstants.HttpRetry,
				HttpPolicyExtensions
						.HandleTransientHttpError()
						.WaitAndRetryAsync(
								policyOptions.HttpRetry.Count,
								retryAttempt => TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));

		// Add circuit breaker policy
		policyRegistry.Add(
				PolicyConstants.HttpCircuitBreaker,
				HttpPolicyExtensions
						.HandleTransientHttpError()
						.CircuitBreakerAsync(
								handledEventsAllowedBeforeBreaking: policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
								durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));

		return services;
	}
}

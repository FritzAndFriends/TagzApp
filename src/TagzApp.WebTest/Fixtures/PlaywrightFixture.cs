using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TagzApp.Web;
using Xunit.Sdk;

namespace TagzApp.WebTest.Fixtures;

/// <summary>
/// WebApplicationFactory that wraps the TestHost in a Kestrel server and provides Playwright and HttpClient testing. 
/// This also logs output from the Host under test to Xunit.
/// 
/// <p>
/// Credit to <a href="https://github.com/CZEMacLeod">https://github.com/CZEMacLeod</a> for writing this. 
/// Functionality is now wrapped in the nuget package C3D.Extensions.Playwright.AspNetCore.Xunit
/// </p>
/// </summary>
public class PlaywrightFixture : PlaywrightFixture<Web.Program>
{
  public override string? Environment { get; } = "Development";

  public PlaywrightFixture(IMessageSink output) : base(output) { }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    //ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };

    builder.UseOnlyStubSocialMediaProvider();

    var host = base.CreateHost(builder);

    return host;
  }
}
﻿using C3D.Extensions.Playwright.AspNetCore.Xunit;

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
public class PlaywrightWebApplicationFactory : PlaywrightFixture<Web.Program>
{
  public override string? Environment { get; } = "Development";

  public PlaywrightWebApplicationFactory(IMessageSink output) : base(output) { }

  protected override IHostBuilder? CreateHostBuilder()
  {
    ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };
    return base.CreateHostBuilder();
  }
}
# Provider Configuration Pattern with IOptionsMonitor

This document describes the standardized configuration pattern for TagzApp providers using `IOptionsMonitor<T>` for reactive configuration updates.

## Overview

The new configuration pattern provides:
- **Immediate Configuration Updates**: Changes to configuration immediately affect the provider
- **Self-Contained Configuration**: Each provider configuration handles its own persistence
- **Testable Architecture**: Clean separation between production and testing scenarios
- **Reactive Updates**: Automatic notification when configuration changes

## Key Components

### 1. BaseProviderConfiguration<T>
Located in `TagzApp.Common.Client`, this abstract base class provides:
- `LoadFromConfigurationAsync()` - Load configuration from persistence
- `SaveToConfigurationAsync()` - Save configuration to persistence  
- `UpdateFromConfiguration()` - Update properties from another instance
- `CreateFromConfigurationAsync<T>()` - Factory method for creating instances

### 2. StaticOptionsMonitor<T>
Located in `TagzApp.Common.Testing`, this class provides:
- Static configuration values for testing scenarios
- No reactive change notifications (suitable for unit tests)
- Implements `IOptionsMonitor<T>` interface for compatibility

### 3. OptionsMonitorExtensions
Located in `TagzApp.Common.Testing`, provides helper methods:
- `ToStaticMonitor()` - Convert `IOptions<T>` to `IOptionsMonitor<T>` for testing
- `CreateStaticMonitor()` - Create static monitor from configuration value

## Implementation Steps

### Step 1: Update Configuration Class

Make your configuration class inherit from `BaseProviderConfiguration<T>`:

```csharp
public class YourProviderConfiguration : BaseProviderConfiguration<YourProviderConfiguration>
{
    public override string ConfigurationKey => "YourProvider";
    
    // Your configuration properties
    public string ApiKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    
    // Update method to copy properties from another instance
    public void UpdateFrom(YourProviderConfiguration other)
    {
        ApiKey = other.ApiKey;
        Enabled = other.Enabled;
        // Copy other properties...
    }
}
```

### Step 2: Create Configuration Setup Class

Create a setup class for async configuration loading:

```csharp
public class YourProviderConfigurationSetup : IConfigureOptions<YourProviderConfiguration>
{
    private readonly IConfigureTagzAppFactory _factory;

    public YourProviderConfigurationSetup(IConfigureTagzAppFactory factory)
    {
        _factory = factory;
    }

    public void Configure(YourProviderConfiguration options)
    {
        var loadedConfig = YourProviderConfiguration.CreateFromConfigurationAsync<YourProviderConfiguration>(_factory).Result;
        options.UpdateFrom(loadedConfig);
    }
}
```

### Step 3: Update Service Registration

In your `StartYourProvider.cs` file:

```csharp
public static class StartYourProvider
{
    public static IServiceCollection AddYourProvider(this IServiceCollection services)
    {
        // Register configuration setup
        services.AddSingleton<IConfigureOptions<YourProviderConfiguration>, YourProviderConfigurationSetup>();
        
        // Configure options with the setup class
        services.Configure<YourProviderConfiguration>(options => { /* options configured by setup class */ });
        
        // Register your provider
        services.AddSingleton<YourProvider>();
        
        return services;
    }
}
```

### Step 4: Update Provider Constructor

Update your provider to use `IOptionsMonitor<T>`:

```csharp
public class YourProvider : ISocialMediaProvider, IDisposable
{
    private readonly IOptionsMonitor<YourProviderConfiguration> _configMonitor;
    private readonly IDisposable? _configChangeSubscription;

    // Production constructor
    public YourProvider(IOptionsMonitor<YourProviderConfiguration> configMonitor, ...)
    {
        _configMonitor = configMonitor;
        
        // Subscribe to configuration changes
        _configChangeSubscription = _configMonitor.OnChange(async (config, name) =>
        {
            await HandleConfigurationChange(config);
        });
    }

    // Testing constructor
    internal YourProvider(IOptions<YourProviderConfiguration> settings, ...)
    {
        _configMonitor = settings.ToStaticMonitor(); // Uses extension method
        _configChangeSubscription = null; // No change subscription for testing
        // ... other test setup
    }

    private async Task HandleConfigurationChange(YourProviderConfiguration newConfig)
    {
        var previousConfig = _configMonitor.CurrentValue;
        
        // Handle specific configuration changes
        if (previousConfig.Enabled != newConfig.Enabled)
        {
            if (newConfig.Enabled)
                await StartAsync();
            else
                await StopAsync();
        }
        
        // Handle other configuration changes...
    }
}
```

### Step 5: Update Configuration Methods

Update `GetConfiguration` and `SaveConfiguration` methods:

```csharp
public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
{
    return await BaseProviderConfiguration<YourProviderConfiguration>.CreateFromConfigurationAsync<YourProviderConfiguration>(configure);
}

public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
{
    var config = (YourProviderConfiguration)providerConfiguration;
    await config.SaveToConfigurationAsync(configure);
    
    // The IOptionsMonitor will automatically pick up the changes from the saved configuration
    // No need to manually update since it's reactive
}
```

## Benefits

1. **Reactive Configuration**: Changes automatically trigger provider updates
2. **Self-Contained**: Configuration classes handle their own persistence logic
3. **Testable**: Clean separation between production and testing scenarios
4. **Consistent**: Standardized pattern across all providers
5. **Immediate Updates**: No restart required for configuration changes

## Testing

For unit tests, use the testing constructor with `IOptions<T>`:

```csharp
var options = Options.Create(new YourProviderConfiguration { Enabled = true });
var provider = new YourProvider(options, logger, mockDependency);
```

The `ToStaticMonitor()` extension method automatically handles the conversion to `IOptionsMonitor<T>`.

## Migration Checklist

- [ ] Update configuration class to inherit from `BaseProviderConfiguration<T>`
- [ ] Create configuration setup class implementing `IConfigureOptions<T>`
- [ ] Update service registration to use new pattern
- [ ] Update provider constructor to use `IOptionsMonitor<T>`
- [ ] Add configuration change handler method
- [ ] Update `GetConfiguration` and `SaveConfiguration` methods
- [ ] Test reactive configuration updates
- [ ] Verify unit tests still work with testing constructor

# TagzApp Custom Error UI

This document describes the custom error user interface implemented for TagzApp, replacing the default yellow Blazor reconnection bar with a modern, integrated solution.

## Components

### 1. CustomErrorBoundary.razor
- **Location**: `TagzApp.Blazor/Components/Shared/CustomErrorBoundary.razor`
- **Purpose**: Catches and displays unhandled exceptions in a user-friendly way
- **Features**:
  - Modern card-based design matching TagzApp's aesthetic
  - Error details toggle for debugging
  - Recovery options (refresh, try to continue)
  - Integration with toast notifications for less critical errors
  - Request ID tracking for support purposes

### 2. ConnectionStatusHandler.razor
- **Location**: `TagzApp.Blazor/Components/Shared/ConnectionStatusHandler.razor`
- **Purpose**: Monitors and handles Blazor/SignalR connection status
- **Features**:
  - Real-time connection monitoring
  - Automatic reconnection attempts with progress indication
  - Manual reconnection options
  - Graceful handling of network issues

### 3. JavaScript Integration
- **connection-handler.js**: Monitors Blazor and SignalR connection events
- **signalr-monitor.js**: Registers SignalR connections for monitoring
- **Enhanced App.razor**: Disables default Blazor reconnection UI

## Design Philosophy

The custom error UI follows TagzApp's design principles:

1. **Consistent Styling**: Uses the same rounded corners (20px), spacing, and color scheme as social media cards
2. **Bootstrap Integration**: Leverages existing Bootstrap dark theme and icon system
3. **Responsive Design**: Works well on desktop and mobile devices
4. **Accessibility**: Proper ARIA labels and keyboard navigation
5. **User-Centric**: Clear messaging and helpful recovery actions

## Visual Features

- **Backdrop Blur**: Modern glassmorphism effect with backdrop-filter
- **Smooth Animations**: Fade-in and slide-in effects for better user experience
- **Icon System**: Bootstrap Icons for consistent iconography
- **Color Coding**: Warning colors for errors, success colors for recovery
- **Progressive Disclosure**: Optional error details that can be toggled

## Integration Points

### MainLayout.razor
```razor
<CustomErrorBoundary>
    <!-- Main application content -->
</CustomErrorBoundary>
<ConnectionStatusHandler />
```

### App.razor
Enhanced JavaScript to disable default Blazor UI and prepare SignalR monitoring.

### CSS Integration
Custom styles added to `site.css` that complement existing TagzApp styles.

## Error Handling Strategy

1. **Error Boundary**: Catches component-level exceptions
2. **Toast Notifications**: Shows less critical errors and recovery messages
3. **Connection Monitoring**: Handles network and SignalR issues
4. **Graceful Degradation**: Provides fallback options when recovery fails

## Usage Examples

### Basic Error Boundary
Wrap any component that might throw exceptions:
```razor
<CustomErrorBoundary>
    <YourComponent />
</CustomErrorBoundary>
```

### Toast Notifications
Show user-friendly error messages:
```csharp
ToastService.Add("Operation failed. Please try again.", MessageSeverity.Warning);
```

### Connection Monitoring
The ConnectionStatusHandler automatically monitors connections when added to the layout.

## Testing

A demo page is available at `/error-demo` to test:
- Error boundary functionality
- Toast notification types
- Visual appearance and behavior

## Configuration

No additional configuration is required. The error UI:
- Automatically integrates with existing TagzApp styling
- Uses the current Bootstrap theme (dark/light)
- Respects existing color schemes and spacing
- Works with all current TagzApp features

## Browser Support

The custom error UI supports:
- Modern browsers with CSS backdrop-filter support
- Graceful fallback for older browsers
- Mobile and desktop responsive layouts
- High contrast and accessibility modes

## Future Enhancements

Potential future improvements:
- Integration with external error reporting services
- User feedback collection on errors
- Advanced retry strategies for different error types
- Telemetry integration for error analytics
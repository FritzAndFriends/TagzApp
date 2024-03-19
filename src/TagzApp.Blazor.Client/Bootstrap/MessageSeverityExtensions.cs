namespace TagzApp.Blazor.Client.Bootstrap;

public static class MessageSeverityExtensions
{

	public static string ToBackgroundColorCss(this MessageSeverity severity)
		=> severity switch
		{
			MessageSeverity.Info => "bg-info",
			MessageSeverity.Success => "bg-success",
			MessageSeverity.Warning => "bg-warning",
			MessageSeverity.Danger => "bg-danger",
			_ => string.Empty
		};

	public static string ToTextColorCss(this MessageSeverity severity)
		=> severity switch
		{
			MessageSeverity.Info => "text-info",
			MessageSeverity.Success => "text-success",
			MessageSeverity.Warning => "text-warning",
			MessageSeverity.Danger => "text-danger",
			_ => string.Empty
		};

	public static string ToHeaderText(this MessageSeverity severity)
		=> severity switch
		{
			MessageSeverity.Info => "Info",
			MessageSeverity.Success => "Success",
			MessageSeverity.Danger => "Error",
			MessageSeverity.Warning => "Warning",
			_ => "Message"
		};

	public static string ToHeaderIconCss(this MessageSeverity severity)
		=> severity switch
		{
			MessageSeverity.Warning => "bi-exclamation-circle-fill",
			MessageSeverity.Danger => "bi-x-circle-fill",
			MessageSeverity.Success => "bi-check-circle-fill",
			MessageSeverity.Normal => "bi-envelope",
			_ => "bi-info-circle-fill",
		};
}

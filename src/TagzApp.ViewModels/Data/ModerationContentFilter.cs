namespace TagzApp.ViewModels.Data;

public static class ModerationContentFilter
{
	public const int AllStates = -1;

	public static string GetContentKey(string provider, string providerId) => $"{provider}:{providerId}";

	public static string GetContentKey(ModerationContentModel content) => GetContentKey(content.Provider, content.ProviderId);

	public static bool ShouldDisplay(int filterState, ModerationState contentState) =>
		filterState == AllStates || filterState == (int)contentState;

	public static void ApplyModerationUpdate(
		IDictionary<string, ModerationContentModel> content,
		ModerationContentModel updatedContent,
		int filterState)
	{
		var contentKey = GetContentKey(updatedContent);
		if (!ShouldDisplay(filterState, updatedContent.State))
		{
			content.Remove(contentKey);
			return;
		}

		content[contentKey] = updatedContent;
	}
}

using Azure.AI.ContentSafety;

namespace TagzApp.Storage.Postgres.SafetyModeration;

public static class ContentSafetyExtensions
{

	public static int? CategoryResult(this AnalyzeTextResult result, TextCategory category)
	{

		return result.CategoriesAnalysis.FirstOrDefault(a => a.Category == category)?.Severity ?? null;


	}


}

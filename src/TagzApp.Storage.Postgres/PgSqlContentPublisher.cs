using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

public class PgSqlContentPublisher : IContentPublisher
{

	private readonly PgSqlContext _Context;

	public PgSqlContentPublisher(PgSqlContext context)
	{
		_Context = context;
	}

	public async Task PublishContentAsync(Hashtag tag, Content newContent)
	{

		var existingContent = await _Context.Contents
				.Where(c => c.Provider == newContent.Provider && c.ProviderId == newContent.ProviderId)
				.AnyAsync();

		// content already exists, return or do something else
		if (existingContent) return;

		newContent.HashtagSought = tag.Text;
		_Context.Contents.Add(newContent);
		await _Context.SaveChangesAsync();

	}
}

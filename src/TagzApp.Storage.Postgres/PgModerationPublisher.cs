namespace TagzApp.Storage.Postgres;

public class PgModerationPublisher : IModerationPublisher
{
	private readonly PgSqlContext _Context;

	public PgModerationPublisher(PgSqlContext context)
	{
		_Context = context;
	}


	public async Task ModerateContentAsync(Content newContent, ModerationState state, string user, DateTimeOffset dateStamp)
	{

		var modContent = new ModerationAction()
		{
			Provider = newContent.Provider,
			ProviderId = newContent.ProviderId,
			State = state,
			UserId = user,
			Timestamp = dateStamp
		};
		_Context.ModerationActions.Add(modContent);
		await _Context.SaveChangesAsync();

	}


}

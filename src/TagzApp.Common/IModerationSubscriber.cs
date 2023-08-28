using Microsoft.Extensions.Hosting;

namespace TagzApp.Common;
public interface IModerationSubscriber : IHostedService
{

	/// <summary>
	/// Register for notifications of moderation actions on content
	/// </summary>
	/// <param name="tag">The tag to listen for moderation actions</param>
	/// <param name="onContentModerated">The action to take when content is moderated</param>
	/// <returns></returns>
	Task RegisterForNotification(string tag, Action<ModerationAction, Content> onContentModerated);

	/// <summary>
	/// Notify subscribers of a moderation action
	/// </summary>
	/// <param name="tag">Tag on which the moderation occurred</param>
	/// <param name="action">The action taken</param>
	/// <param name="content">The content that was acted on</param>
	void NotifyOfModeration(string tag, ModerationAction action, Content content);

}

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class WaterfallModel : PageModel
{
	private readonly IMessagingService _Service;
	private readonly ApplicationConfiguration _AppConfiguration;

	public WaterfallModel(IMessagingService service, ApplicationConfiguration AppConfiguration)
	{
		_Service = service;
		_AppConfiguration = AppConfiguration;
	}

	public List<string> Tags { get; } = new List<string>();

	public void OnGet()
	{

		Tags.AddRange(_Service.TagsTracked);


	}

	public string WaterfallHeaderContent => Markdig.Markdown.ToHtml(_AppConfiguration.WaterfallHeaderMarkdown);

	public string WaterfallHeaderCss
	{
		get
		{
			var theCSS = _AppConfiguration.WaterfallHeaderCss;
			if (string.IsNullOrWhiteSpace(theCSS))
			{
				return "";
			}

			var theRules = theCSS.Split('}', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
			var outCSS = new StringBuilder();
			foreach (var rule in theRules)
			{

				if (!rule.StartsWith(".waterfallHeader"))
				{
					outCSS.Append(".waterfallHeader ");
				}

				rule.Replace(Environment.NewLine, "");
				outCSS.Append(rule);
				outCSS.Append("}");
				outCSS.Append(Environment.NewLine);

			}

			return outCSS.ToString();

		}
	}

}

using Microsoft.Extensions.Logging;
using TagzApp.Common.Models;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Formatters;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Models;

namespace TagzApp.Providers.Blazot.Converters;

public class ContentConverter : IContentConverter
{
	private readonly ILogger<ContentConverter> _Logger;

	public ContentConverter(ILogger<ContentConverter> logger)
	{
		_Logger = logger;
	}

	public IEnumerable<Content> ConvertToContent(List<Transmission>? transmissions, Hashtag tag)
	{
		if (transmissions is null || !transmissions.Any())
			return Enumerable.Empty<Content>();

		var outContent = new List<Content>();

		foreach (var transmission in transmissions)
		{
			var content = BuildContentFromTransmission(transmission, tag);
			if (content == null) continue;

			outContent.Add(content);
		}

		return outContent;
	}

	private Content? BuildContentFromTransmission(Transmission? transmission, Hashtag? tag = null)
	{
		if (transmission is null) return null;
		var author = transmission.Author;

		try
		{
			var body = transmission.Body;
			if (!string.IsNullOrWhiteSpace(body))
			{
				body = LinkFormatters.AddWebLinks(body);
				body = LinkFormatters.AddHashTagLinks(body);
				body = LinkFormatters.AddMentionLinks(body);
			}

			body = body.Replace("\n", "<br/>");

			var content = new Content
			{
				Provider = BlazotConstants.ProviderId,
				ProviderId = transmission.TransmissionId.ToString(),
				Author = new Creator
				{
					DisplayName = author.DisplayName,
					UserName = $"@{author.UserName}",
					ProviderId = BlazotConstants.ProviderId,
					ProfileImageUri = new Uri(author.ProfileImageUrl),
					ProfileUri = new Uri(Path.Combine(BlazotConstants.BaseAppAddress, author.UserName).Replace(@"\", "/"))
				},
				SourceUri = new Uri(Path
					.Combine(BlazotConstants.BaseAppAddress, "transmission", transmission.TransmissionId.ToString())
					.Replace(@"\", "/")),
				Text = body,
				Timestamp = new DateTimeOffset(transmission.DateTransmitted, TimeSpan.Zero).ToLocalTime(),
				HashtagSought = tag?.Text ?? string.Empty,
				Type = ContentType.Message
			};

			var imageLinks = transmission.Media.Select(m => m.ImageUrl).ToList();
			if (imageLinks.Any())
			{
				foreach (var link in imageLinks)
				{
					if (link == null) continue;

					content.PreviewCard = new Card
					{
						ImageUri = new Uri(link)
					};
				}
			}

			var videoLinks = transmission.Media.Select(m => m.VideoCover).ToList();
			if (videoLinks.Any())
			{
				foreach (var link in videoLinks)
				{
					if (link == null) continue;

					content.PreviewCard = new Card
					{
						ImageUri = new Uri(link)
					};
				}
			}

			if (!string.IsNullOrWhiteSpace(transmission.WebLink.ImageUrl))
			{
				content.PreviewCard = new Card
				{
					AltText = transmission.WebLink.Description,
					ImageUri = new Uri(transmission.WebLink.ImageUrl)
				};
			}

			return content;
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "{message}", ex.Message);
		}

		return null;
	}
}
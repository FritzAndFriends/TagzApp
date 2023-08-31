using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagzApp.Common;
public class ModeratorCard
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public string? Url { get; set; }
	public string? Color { get; set; }
	public string? BackgroundColor { get; set; }
	public string? BorderColor { get; set; }
	public string? TextColor { get; set; }
	public string? IconUrl { get; set; }
	public string? IconBackgroundColor { get; set; }
	public string? IconBorderColor { get; set; }
	public string? IconTextColor { get; set; }
	public string? Icon { get; set; }
	public string? IconType { get; set; }
	public string? IconSize { get; set; }
	public string? IconPosition { get; set; }
	public string? IconAlignment { get; set; }
	public string? IconColor { get; set; }

	public ModeratorCard()
	{
		#if DEBUG
		this.Name = "Moderator Card";
		this.Description = "This is a moderator card.";
		this.ImageUrl = "https://static-cdn.jtvnw.net/jtv_user_pictures/0f9f9f9f-0f9f-0f9f-0f9f-0f9f0f9f0f9f-profile_image-300x300.png";
		this.Url = "https://www.twitch.tv/";
		this.Color = "#000000";
		this.BackgroundColor = "#ffffff";
		this.BorderColor = "#000000";
		this.TextColor = "#000000";
		this.IconUrl = "https://static-cdn.jtvnw.net/jtv_user_pictures/0f9f9f9f-0f9f-0f9f-0f9f-0f9f0f9f0f9f-profile_image-300x300.png";
		this.IconBackgroundColor = "#ffffff";
		this.IconBorderColor = "#000000";
		this.IconTextColor = "#000000";
		this.Icon = "https://static-cdn.jtvnw.net/jtv_user_pictures/0f9f9f9f-0f9f-0f9f-0f9f-0f9f0f9f0f9f-profile_image-300x300.png";
		this.IconType = "image";
		this.IconSize = "1em";
		this.IconPosition = "left";
		this.IconAlignment = "center";
		this.IconColor = "#000000";
#endif

	}
}

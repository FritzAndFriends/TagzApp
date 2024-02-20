// Ignore Spelling: Css

using System.Drawing;
using System.Text.Json.Serialization;

namespace TagzApp.Common.Models;

public class ModalConfiguration
{

	private const string CONFIG_KEY = "ModalConfiguration";

	public string Caption { get; set; } = "TagzApp - Your Friendly Social Media App";

	public string FontColor { get; set; }

	public string Font { get; set; }

	public bool FontIsBold { get; set; } = false;

	public string BackgroundColor { get; set; }

	public string BackgroundImage { get; set; }

	public bool BackgroundImageRepeat { get; set; } = false;

	public string BackgroundImageMimeType { get; set; } = "image/png";

	public string BackgroundImagePosition { get; set; } = "center center";

	public string BackgroundImageSize { get; set; } = "cover";

	public string GetCssWithBackgroundImage(string newImage = "", string newImageMimeType = "")
	{

		newImage = newImage == "" ? BackgroundImage : newImage;
		newImageMimeType = newImageMimeType == "" ? BackgroundImageMimeType : newImageMimeType;

		if (string.IsNullOrEmpty(newImage))
		{
			return CssRules;
		}

		return $$"""
				.modal-back {
					background-image: url(data:{{newImageMimeType ?? ""}};base64,{{newImage ?? ""}});
					background-repeat: {{(BackgroundImageRepeat ? "repeat" : "no-repeat")}};
					background-size: {{BackgroundImageSize}};
					background-position: {{BackgroundImagePosition}};
				}

				.modal-back-content {
					background-color: transparent;
				}
			""" + CssRules;
	}

	[JsonIgnore]
	public string CssRules
	{
		get
		{

			return $$"""
					.modal-back {
						background-color: {{BackgroundColor}};
					}

					.modal-back-content {
						width: 100%;
						height: 100%;
						display: flex;
						align-items: center;
						justify-content: center;
						font-family: {{Font}};
						font-weight: {{(FontIsBold ? "bold" : "normal")}};
						color: {{FontColor}};
					}

					.modal-back-content::before {
						content: '{{Caption.Replace("'", "\\'").Replace("\n", "\\A ")}}';
						white-space: break-spaces;
						text-align: center;
					}
				""";
		}
	}

	public static async Task<ModalConfiguration> LoadFromConfiguration(IConfigureTagzApp config)
	{

		return await config.GetConfigurationById<ModalConfiguration>(CONFIG_KEY);

	}

	public async Task SaveConfiguration(IConfigureTagzApp configure)
	{

		await configure.SetConfigurationById(CONFIG_KEY, this);

	}

	private static string ColorToHex(Color color)
	{
		string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
		return hex;
	}

}

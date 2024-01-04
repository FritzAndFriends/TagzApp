using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;

namespace TagzApp.Blazor.Services;

public class JsonTempDataSerializer : TempDataSerializer
{

	public override byte[] Serialize(IDictionary<string, object>? values)
	{
		var hasValues = values?.Count > 0;
		if (!hasValues)
			return Array.Empty<byte>();

		using var memoryStream = new MemoryStream();
		JsonSerializer.Serialize(memoryStream, values);

		return memoryStream.ToArray();
	}

	public override IDictionary<string, object> Deserialize(byte[] unprotectedData)
	{
		using var memoryStream = new MemoryStream(unprotectedData);

		var tempDataDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(memoryStream)
				?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		foreach (var item in tempDataDictionary)
		{
			if (item.Value is JsonElement)
			{

				var key = item.Key.ToString();
				var value = ((JsonElement)item.Value).GetRawText().TrimStart('\"').TrimEnd('\"');
				tempDataDictionary[key] = value;

			}
		}

		return tempDataDictionary;
	}
};

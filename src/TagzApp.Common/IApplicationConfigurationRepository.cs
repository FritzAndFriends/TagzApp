// Ignore Spelling: Css

namespace TagzApp.Common;

public interface IApplicationConfigurationRepository
{
	Task SetValues(ApplicationConfiguration config);

	Task SetValue(string key, string value);

}

namespace TagzApp.Common.Exceptions;

public class InvalidConfigurationException : ApplicationException
{

	public InvalidConfigurationException(string message, string configKey) : base(message)
	{

		Key = configKey;

	}

	public InvalidConfigurationException(string configKey) : base($"Missing configuration with key {configKey}")
	{

		Key = configKey;

	}

	/// <summary>
	/// The missing configuration key
	/// </summary>
	public string Key { get; }

}

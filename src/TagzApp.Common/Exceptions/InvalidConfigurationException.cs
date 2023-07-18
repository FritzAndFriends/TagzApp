namespace TagzApp.Common.Exceptions;

public class InvalidConfigurationException : ApplicationException
{

	public InvalidConfigurationException(string message, string configKey) : base(message) {

		Key = configKey;

	}

	/// <summary>
	/// The missing configuration key
	/// </summary>
  public string Key { get; }

}

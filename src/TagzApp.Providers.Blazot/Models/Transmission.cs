namespace TagzApp.Providers.Blazot.Models;

// TODO: Check CS8618: Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class Transmission
{
	/// <summary>
	/// The unique identifier for the transmission.
	/// </summary>
	public Guid TransmissionId { get; set; }

	/// <summary>
	/// The unique identifier for the parent transmission that this transmission is a response to.
	/// </summary>
	public Guid? ParentItemId { get; set; }

	/// <summary>
	/// The UTC date/time this transmission was transmitted.
	/// </summary>
	public DateTime DateTransmitted { get; set; }

	/// <summary>
	/// The transmission text.
	/// </summary>

	public string Body { get; set; }


	/// <summary>
	/// The author object.
	/// </summary>
	public Author Author { get; set; }

	/// <summary>
	/// Video or images within this transmission.
	/// </summary>
	public List<Media> Media { get; set; }

	public WebLink WebLink { get; set; }

	public Transmission RelayedTransmission { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

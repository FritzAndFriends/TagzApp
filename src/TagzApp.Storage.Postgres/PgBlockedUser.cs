using System.ComponentModel.DataAnnotations;

namespace TagzApp.Storage.Postgres;

public class PgBlockedUser
{

	[Key]
	public int Id { get; set; }

	[Required, MaxLength(20)]
	public string Provider { get; set; }

	[Required, MaxLength(200)]
	public string UserName { get; set; }

	[Required, MaxLength(200)]
	public string BlockingUser { get; set; }

	[Required]
	public DateTimeOffset BlockDateTime { get; set; } = DateTimeOffset.UtcNow;

	public DateTimeOffset ExpirationDateTime { get; set; } = new DateTimeOffset(new DateTime(2050, 1, 1));

	public BlockedUserCapabilities Capabilities { get; set; } = BlockedUserCapabilities.Moderated;

	// Add an explicit operator for converting between a blocked user and a PGblockeduser
	public static implicit operator BlockedUser(PgBlockedUser thisBlockedUser)
	{

		return new BlockedUser
		{

			Provider = thisBlockedUser.Provider,
			UserName = thisBlockedUser.UserName,
			BlockingUser = thisBlockedUser.BlockingUser,
			BlockedDate = thisBlockedUser.BlockDateTime,
			ExpirationDate = thisBlockedUser.ExpirationDateTime,
			Capabilities = thisBlockedUser.Capabilities

		};

	}

}

using System.Security.Cryptography;
using System.Text;

namespace TagzApp.Common;

/// <summary>
/// Provides consistent hashing of user identifiers for database storage.
/// Ensures all user IDs are hashed to fit within database column constraints.
/// </summary>
public static class UserIdHasher
{
	/// <summary>
	/// Creates a SHA256 hash of the user identifier, truncated to 32 characters.
	/// This ensures the HashedUserId fits within the database column constraint.
	/// </summary>
	/// <param name="userIdentifier">The user identifier to hash</param>
	/// <returns>A 32-character hash string</returns>
	public static string CreateHashedUserId(string userIdentifier)
	{
		if (string.IsNullOrWhiteSpace(userIdentifier))
		{
			throw new ArgumentException("User identifier cannot be null or whitespace", nameof(userIdentifier));
		}

		using var sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(userIdentifier));
		var hashString = Convert.ToBase64String(hashBytes)
				.Replace("+", "-")
				.Replace("/", "_")
				.Replace("=", "");

		// Truncate to 32 characters to fit the database column constraint
		return hashString.Length > 32 ? hashString[..32] : hashString;
	}
}

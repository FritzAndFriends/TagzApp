using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Web.Data;

public class TagzAppUser : IdentityUser
{

	[MaxLength(50)]
	public string? DisplayName { get; set; }

}

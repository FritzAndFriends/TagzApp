using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Web.Migrations
{
	/// <inheritdoc />
	public partial class AddDisplayNameToUser : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
					name: "DisplayName",
					table: "AspNetUsers",
					type: "TEXT",
					maxLength: 50,
					nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "DisplayName",
					table: "AspNetUsers");
		}
	}
}

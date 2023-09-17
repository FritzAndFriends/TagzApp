using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
	/// <inheritdoc />
	public partial class AddApplicationConfiguration : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
					name: "Settings",
					columns: table => new
					{
						Id = table.Column<string>(type: "text", nullable: false),
						Value = table.Column<string>(type: "text", nullable: true)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Settings", x => x.Id);
					});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "Settings");
		}
	}
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
	/// <inheritdoc />
	public partial class AddedTagzWatched : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
					name: "TagsWatched",
					columns: table => new
					{
						Text = table.Column<string>(type: "text", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_TagsWatched", x => x.Text);
					});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "TagsWatched");
		}
	}
}

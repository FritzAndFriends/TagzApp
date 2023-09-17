using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
	/// <inheritdoc />
	public partial class MaxSizeForTag : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
					name: "Text",
					table: "TagsWatched",
					type: "character varying(50)",
					maxLength: 50,
					nullable: false,
					oldClrType: typeof(string),
					oldType: "text");

			migrationBuilder.AlterColumn<string>(
					name: "HashtagSought",
					table: "Content",
					type: "character varying(50)",
					maxLength: 50,
					nullable: false,
					oldClrType: typeof(string),
					oldType: "text");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
					name: "Text",
					table: "TagsWatched",
					type: "text",
					nullable: false,
					oldClrType: typeof(string),
					oldType: "character varying(50)",
					oldMaxLength: 50);

			migrationBuilder.AlterColumn<string>(
					name: "HashtagSought",
					table: "Content",
					type: "text",
					nullable: false,
					oldClrType: typeof(string),
					oldType: "character varying(50)",
					oldMaxLength: 50);
		}
	}
}

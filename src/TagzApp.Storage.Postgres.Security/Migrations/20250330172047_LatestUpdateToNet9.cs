using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TagzApp.Storage.Postgres.Security.Migrations;

/// <inheritdoc />
public partial class LatestUpdateToNet9 : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{

		//try
		//{
		//	migrationBuilder.DropTable(
		//			name: "Settings");
		//}
		//catch { }

		migrationBuilder.AlterColumn<string>(
							name: "Name",
							table: "AspNetUserTokens",
							type: "text",
							nullable: false,
							oldClrType: typeof(string),
							oldType: "character varying(128)",
							oldMaxLength: 128);

		migrationBuilder.AlterColumn<string>(
				name: "LoginProvider",
				table: "AspNetUserTokens",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(128)",
				oldMaxLength: 128);

		migrationBuilder.AlterColumn<string>(
				name: "ProviderKey",
				table: "AspNetUserLogins",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(128)",
				oldMaxLength: 128);

		migrationBuilder.AlterColumn<string>(
				name: "LoginProvider",
				table: "AspNetUserLogins",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(128)",
				oldMaxLength: 128);

		migrationBuilder.InsertData(
				table: "AspNetRoles",
				columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
				values: new object[,]
				{
									{ "1", null, "Admin", "ADMIN" },
									{ "2", null, "Moderator", "MODERATOR" }
				});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DeleteData(
				table: "AspNetRoles",
				keyColumn: "Id",
				keyValue: "1");

		migrationBuilder.DeleteData(
				table: "AspNetRoles",
				keyColumn: "Id",
				keyValue: "2");

		migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "AspNetUserTokens",
				type: "character varying(128)",
				maxLength: 128,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

		migrationBuilder.AlterColumn<string>(
				name: "LoginProvider",
				table: "AspNetUserTokens",
				type: "character varying(128)",
				maxLength: 128,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

		migrationBuilder.AlterColumn<string>(
				name: "ProviderKey",
				table: "AspNetUserLogins",
				type: "character varying(128)",
				maxLength: 128,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

		migrationBuilder.AlterColumn<string>(
				name: "LoginProvider",
				table: "AspNetUserLogins",
				type: "character varying(128)",
				maxLength: 128,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

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
}

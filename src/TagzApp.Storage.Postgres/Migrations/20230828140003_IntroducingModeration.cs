using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
	/// <inheritdoc />
	public partial class IntroducingModeration : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<long>(
					name: "ModerationActionId",
					table: "Content",
					type: "bigint",
					nullable: true);

			migrationBuilder.CreateTable(
					name: "ModerationActions",
					columns: table => new
					{
						Id = table.Column<long>(type: "bigint", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						Provider = table.Column<string>(type: "text", nullable: false),
						ProviderId = table.Column<string>(type: "text", nullable: false),
						State = table.Column<int>(type: "integer", nullable: false),
						Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
						Moderator = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
						Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_ModerationActions", x => x.Id);
						table.UniqueConstraint("AK_ModerationActions_Provider_ProviderId", x => new { x.Provider, x.ProviderId });
					});

			migrationBuilder.CreateIndex(
					name: "IX_Content_ModerationActionId",
					table: "Content",
					column: "ModerationActionId");

			migrationBuilder.AddForeignKey(
					name: "FK_Content_ModerationActions_ModerationActionId",
					table: "Content",
					column: "ModerationActionId",
					principalTable: "ModerationActions",
					principalColumn: "Id");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_Content_ModerationActions_ModerationActionId",
					table: "Content");

			migrationBuilder.DropTable(
					name: "ModerationActions");

			migrationBuilder.DropIndex(
					name: "IX_Content_ModerationActionId",
					table: "Content");

			migrationBuilder.DropColumn(
					name: "ModerationActionId",
					table: "Content");
		}
	}
}

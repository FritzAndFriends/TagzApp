using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
	/// <inheritdoc />
	public partial class ModerationActionRelationship : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_Content_ModerationActions_ModerationActionId",
					table: "Content");

			migrationBuilder.DropIndex(
					name: "IX_Content_ModerationActionId",
					table: "Content");

			migrationBuilder.DropColumn(
					name: "ModerationActionId",
					table: "Content");

			migrationBuilder.AddColumn<long>(
					name: "ContentId",
					table: "ModerationActions",
					type: "bigint",
					nullable: false,
					defaultValue: 0L);

			migrationBuilder.CreateIndex(
					name: "IX_ModerationActions_ContentId",
					table: "ModerationActions",
					column: "ContentId",
					unique: true);

			migrationBuilder.AddForeignKey(
					name: "FK_ModerationActions_Content_ContentId",
					table: "ModerationActions",
					column: "ContentId",
					principalTable: "Content",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_ModerationActions_Content_ContentId",
					table: "ModerationActions");

			migrationBuilder.DropIndex(
					name: "IX_ModerationActions_ContentId",
					table: "ModerationActions");

			migrationBuilder.DropColumn(
					name: "ContentId",
					table: "ModerationActions");

			migrationBuilder.AddColumn<long>(
					name: "ModerationActionId",
					table: "Content",
					type: "bigint",
					nullable: true);

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
	}
}

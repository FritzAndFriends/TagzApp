using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemovedBlockedUserAlternateKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_BlockedUsers_Provider_UserName",
                table: "BlockedUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_BlockedUsers_Provider_UserName",
                table: "BlockedUsers",
                columns: new[] { "Provider", "UserName" });
        }
    }
}

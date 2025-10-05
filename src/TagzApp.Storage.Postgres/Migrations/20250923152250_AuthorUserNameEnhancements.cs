using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AuthorUserNameEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // We must drop the generated column before altering the base column type it depends on.
            migrationBuilder.Sql("ALTER TABLE \"Content\" DROP COLUMN IF EXISTS \"AuthorUserName\";");

            // Convert Author column text -> jsonb using explicit cast
            migrationBuilder.Sql("ALTER TABLE \"Content\" ALTER COLUMN \"Author\" TYPE jsonb USING \"Author\"::jsonb;");

            // Recreate generated column (now referencing jsonb directly)
            migrationBuilder.AddColumn<string>(
                name: "AuthorUserName",
                table: "Content",
                type: "text",
                nullable: true,
                computedColumnSql: "lower((\"Author\" ->> 'UserName'))",
                stored: true);

            // Index for faster lookups (provider + author + timestamp)
            migrationBuilder.CreateIndex(
                name: "IX_Content_Provider_AuthorUserName_Timestamp",
                table: "Content",
                columns: new[] { "Provider", "AuthorUserName", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop index first
            migrationBuilder.DropIndex(
                name: "IX_Content_Provider_AuthorUserName_Timestamp",
                table: "Content");

            // Drop generated column so we can revert Author type
            migrationBuilder.Sql("ALTER TABLE \"Content\" DROP COLUMN IF EXISTS \"AuthorUserName\";");

            // Revert jsonb back to text
            migrationBuilder.Sql("ALTER TABLE \"Content\" ALTER COLUMN \"Author\" TYPE text USING \"Author\"::text;");

            // Recreate the original generated column definition (with jsonb cast for compatibility if migration chain is replayed)
            migrationBuilder.AddColumn<string>(
                name: "AuthorUserName",
                table: "Content",
                type: "text",
                nullable: true,
                computedColumnSql: "lower((\"Author\"::jsonb ->> 'UserName'))",
                stored: true);
        }
    }
}

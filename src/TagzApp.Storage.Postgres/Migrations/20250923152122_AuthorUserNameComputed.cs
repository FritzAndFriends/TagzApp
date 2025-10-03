using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AuthorUserNameComputed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorUserName",
                table: "Content",
                type: "text",
                nullable: true,
                computedColumnSql: "lower((\"Author\"::jsonb ->> 'UserName'))",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorUserName",
                table: "Content");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ConfigValue1 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue2 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue3 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue4 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue5 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue6 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue7 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue8 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue9 = table.Column<string>(type: "text", nullable: true),
                    ConfigValue10 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderConfigurations");
        }
    }
}

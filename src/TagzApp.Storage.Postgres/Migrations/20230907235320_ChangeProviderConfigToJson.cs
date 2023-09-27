using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProviderConfigToJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigValue1",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue10",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue2",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue3",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue4",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue5",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue6",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue7",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue8",
                table: "ProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigValue9",
                table: "ProviderConfigurations");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "ConfigurationSettings",
                table: "ProviderConfigurations",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigurationSettings",
                table: "ProviderConfigurations");

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue1",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue10",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue2",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue3",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue4",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue5",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue6",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue7",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue8",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue9",
                table: "ProviderConfigurations",
                type: "text",
                nullable: true);
        }
    }
}

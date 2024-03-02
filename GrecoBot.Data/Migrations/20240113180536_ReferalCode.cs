using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrecoBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReferalCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferalCode",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SummOfTransactions",
                table: "User",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferalCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SummOfTransactions",
                table: "User");
        }
    }
}

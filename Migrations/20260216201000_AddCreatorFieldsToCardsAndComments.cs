using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Retroboard.Api.Infrastructure.Data;

#nullable disable

namespace Retroboard.Api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(RetroboardDbContext))]
    [Migration("20260216201000_AddCreatorFieldsToCardsAndComments")]
    public partial class AddCreatorFieldsToCardsAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByDisplayName",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByDisplayName",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByDisplayName",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CreatedByDisplayName",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Comments");
        }
    }
}

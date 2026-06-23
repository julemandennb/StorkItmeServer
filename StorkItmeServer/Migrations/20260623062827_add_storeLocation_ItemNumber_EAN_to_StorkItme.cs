using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class add_storeLocation_ItemNumber_EAN_to_StorkItme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EAN",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "storeLocation",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EAN",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "storeLocation",
                schema: "storkitmeserver",
                table: "StorkItme");
        }
    }
}

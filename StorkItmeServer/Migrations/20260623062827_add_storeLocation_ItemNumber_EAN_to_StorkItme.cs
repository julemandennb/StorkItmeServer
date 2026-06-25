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
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "StoreLocation",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: true,
                defaultValue: null);

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "ItemNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_EAN",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "EAN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
               name: "IX_StorkItme_ItemNumber",
               schema: "storkitmeserver",
               table: "StorkItme");

            migrationBuilder.DropIndex(
                name: "IX_StorkItme_EAN",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "EAN",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "StoreLocation",
                schema: "storkitmeserver",
                table: "StorkItme");
        }
    }
}

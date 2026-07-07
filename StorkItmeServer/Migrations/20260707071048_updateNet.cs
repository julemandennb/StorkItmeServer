using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class updateNet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StorkItmeGroup_Name",
                schema: "storkitmeserver",
                table: "StorkItmeGroup",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StorkItmeGroup_Uuid",
                schema: "storkitmeserver",
                table: "StorkItmeGroup",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorkItmeGroup_Name",
                schema: "storkitmeserver",
                table: "StorkItmeGroup");

            migrationBuilder.DropIndex(
                name: "IX_StorkItmeGroup_Uuid",
                schema: "storkitmeserver",
                table: "StorkItmeGroup");
        }
    }
}

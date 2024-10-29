using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class RoleCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "storkitmeserver",
                table: "AspNetRoles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "storkitmeserver",
                table: "AspNetRoles");
        }
    }
}

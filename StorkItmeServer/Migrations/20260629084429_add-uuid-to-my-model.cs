using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class adduuidtomymodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "UserGroup",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "StorkItmeGroup",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "UserGroup");

            migrationBuilder.DropColumn(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "StorkItmeGroup");

            migrationBuilder.DropColumn(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "AspNetUsers");
        }
    }
}

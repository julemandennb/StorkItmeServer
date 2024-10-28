using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class StorkItmeCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorkItme",
                schema: "storkitmeserver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    BestBy = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stork = table.Column<int>(type: "integer", nullable: false),
                    ImgUrl = table.Column<string>(type: "text", nullable: false),
                    UserGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorkItme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorkItme_UserGroup_Id",
                        column: x => x.Id,
                        principalSchema: "storkitmeserver",
                        principalTable: "UserGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_Name",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_Stork",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "Stork");

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_Type",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorkItme",
                schema: "storkitmeserver");
        }
    }
}

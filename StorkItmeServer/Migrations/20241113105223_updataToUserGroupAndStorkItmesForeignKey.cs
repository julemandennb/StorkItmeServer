using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class updataToUserGroupAndStorkItmesForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorkItme_UserGroup_Id",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_UserGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_StorkItme_UserGroup_UserGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "UserGroupId",
                principalSchema: "storkitmeserver",
                principalTable: "UserGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorkItme_UserGroup_UserGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropIndex(
                name: "IX_StorkItme_UserGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.AddForeignKey(
                name: "FK_StorkItme_UserGroup_Id",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "Id",
                principalSchema: "storkitmeserver",
                principalTable: "UserGroup",
                principalColumn: "Id");
        }
    }
}

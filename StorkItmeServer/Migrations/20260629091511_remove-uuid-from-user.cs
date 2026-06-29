using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class removeuuidfromuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorkItme_StorkItmeGroup_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_Uuid",
                schema: "storkitmeserver",
                table: "UserGroup",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_Uuid",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "Uuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StorkItme_StorkItmeGroup_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "StorkItmeGroupId",
                principalSchema: "storkitmeserver",
                principalTable: "StorkItmeGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorkItme_StorkItmeGroup_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropIndex(
                name: "IX_UserGroup_Uuid",
                schema: "storkitmeserver",
                table: "UserGroup");

            migrationBuilder.DropIndex(
                name: "IX_StorkItme_Uuid",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.AlterColumn<int>(
                name: "StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                schema: "storkitmeserver",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_StorkItme_StorkItmeGroup_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "StorkItmeGroupId",
                principalSchema: "storkitmeserver",
                principalTable: "StorkItmeGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

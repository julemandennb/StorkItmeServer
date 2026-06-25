using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStorkItmeGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "StoreLocation",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ImgUrl",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EAN",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StorkItmeGroup",
                schema: "storkitmeserver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorkItmeGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorkItmeGroupUser",
                schema: "storkitmeserver",
                columns: table => new
                {
                    StorkItmeGroupsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorkItmeGroupUser", x => new { x.StorkItmeGroupsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_StorkItmeGroupUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "storkitmeserver",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StorkItmeGroupUser_StorkItmeGroup_StorkItmeGroupsId",
                        column: x => x.StorkItmeGroupsId,
                        principalSchema: "storkitmeserver",
                        principalTable: "StorkItmeGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorkItme_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                column: "StorkItmeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StorkItmeGroupUser_UsersId",
                schema: "storkitmeserver",
                table: "StorkItmeGroupUser",
                column: "UsersId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorkItme_StorkItmeGroup_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropTable(
                name: "StorkItmeGroupUser",
                schema: "storkitmeserver");

            migrationBuilder.DropTable(
                name: "StorkItmeGroup",
                schema: "storkitmeserver");

            migrationBuilder.DropIndex(
                name: "IX_StorkItme_StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.DropColumn(
                name: "StorkItmeGroupId",
                schema: "storkitmeserver",
                table: "StorkItme");

            migrationBuilder.AlterColumn<int>(
                name: "UserGroupId",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StoreLocation",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemNumber",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImgUrl",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EAN",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}

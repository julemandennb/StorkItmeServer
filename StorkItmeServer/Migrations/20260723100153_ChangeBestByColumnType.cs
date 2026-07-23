using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorkItmeServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBestByColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "BestBy",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "BestBy",
                schema: "storkitmeserver",
                table: "StorkItme",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}

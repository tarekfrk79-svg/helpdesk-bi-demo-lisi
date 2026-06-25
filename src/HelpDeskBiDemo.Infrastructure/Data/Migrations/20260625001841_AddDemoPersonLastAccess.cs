using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskBiDemo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoPersonLastAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignedInAtUtc",
                table: "DemoPeople",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSignedInAtUtc",
                table: "DemoPeople");
        }
    }
}

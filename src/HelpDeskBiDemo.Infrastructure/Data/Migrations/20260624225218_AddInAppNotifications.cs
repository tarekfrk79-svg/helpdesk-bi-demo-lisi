using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskBiDemo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInAppNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    RecipientRole = table.Column<int>(type: "int", nullable: false),
                    RecipientPersonId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_DemoPeople_RecipientPersonId",
                        column: x => x.RecipientPersonId,
                        principalTable: "DemoPeople",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyId_RecipientRole_IsRead_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "CompanyId", "RecipientRole", "IsRead", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientPersonId_IsRead_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "RecipientPersonId", "IsRead", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TicketId",
                table: "Notifications",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}

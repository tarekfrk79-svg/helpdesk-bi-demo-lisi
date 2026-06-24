using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskBiDemo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketHistoryAndSlaTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAtUtc",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    ActorPersonId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketActivities_DemoPeople_ActorPersonId",
                        column: x => x.ActorPersonId,
                        principalTable: "DemoPeople",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketActivities_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivities_ActorPersonId",
                table: "TicketActivities",
                column: "ActorPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivities_TicketId_CreatedAtUtc",
                table: "TicketActivities",
                columns: new[] { "TicketId", "CreatedAtUtc" });

            migrationBuilder.Sql(
                """
                UPDATE Tickets
                SET AssignedAtUtc = CreatedAtUtc
                WHERE AssignedTechnicianId IS NOT NULL
                  AND AssignedAtUtc IS NULL;

                INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
                SELECT
                    t.Id,
                    1,
                    t.CreatedByPersonId,
                    CONCAT(
                        N'Ticket cree avec la categorie ',
                        CASE t.Category
                            WHEN 1 THEN N'Logiciel'
                            WHEN 2 THEN N'Materiel'
                            WHEN 3 THEN N'Acces'
                            WHEN 4 THEN N'Bug'
                            ELSE N'Autre'
                        END,
                        N' et la priorite ',
                        CASE t.Priority
                            WHEN 1 THEN N'Basse'
                            WHEN 2 THEN N'Normale'
                            WHEN 3 THEN N'Haute'
                            WHEN 4 THEN N'Urgente'
                            ELSE N'Normale'
                        END,
                        N'.'
                    ),
                    t.CreatedAtUtc
                FROM Tickets t;

                INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
                SELECT
                    t.Id,
                    2,
                    t.AssignedTechnicianId,
                    CONCAT(N'Ticket assigne a ', technician.FullName, N'.'),
                    COALESCE(t.AssignedAtUtc, t.CreatedAtUtc)
                FROM Tickets t
                INNER JOIN DemoPeople technician ON technician.Id = t.AssignedTechnicianId
                WHERE t.AssignedTechnicianId IS NOT NULL;

                INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
                SELECT
                    t.Id,
                    3,
                    COALESCE(t.AssignedTechnicianId, t.CreatedByPersonId),
                    CONCAT(
                        N'Statut passe de Nouveau a ',
                        CASE t.Status
                            WHEN 2 THEN N'En cours'
                            WHEN 3 THEN N'Resolu'
                            WHEN 4 THEN N'Clos'
                            ELSE N'Nouveau'
                        END,
                        N'. Action effectuee par ',
                        COALESCE(technician.FullName, requester.FullName),
                        N'.'
                    ),
                    COALESCE(t.ResolvedAtUtc, t.UpdatedAtUtc, t.CreatedAtUtc)
                FROM Tickets t
                INNER JOIN DemoPeople requester ON requester.Id = t.CreatedByPersonId
                LEFT JOIN DemoPeople technician ON technician.Id = t.AssignedTechnicianId
                WHERE t.Status <> 1;

                INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
                SELECT
                    comment.TicketId,
                    4,
                    comment.AuthorPersonId,
                    N'Commentaire ajoute sur le ticket.',
                    comment.CreatedAtUtc
                FROM TicketComments comment;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketActivities");

            migrationBuilder.DropColumn(
                name: "AssignedAtUtc",
                table: "Tickets");
        }
    }
}

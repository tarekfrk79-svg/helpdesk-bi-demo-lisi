using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskBiDemo.Infrastructure.Data.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(x => new { x.CompanyId, x.Status });

        builder.HasIndex(x => new { x.CompanyId, x.Category });

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByPerson)
            .WithMany(x => x.CreatedTickets)
            .HasForeignKey(x => x.CreatedByPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedTechnician)
            .WithMany(x => x.AssignedTickets)
            .HasForeignKey(x => x.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

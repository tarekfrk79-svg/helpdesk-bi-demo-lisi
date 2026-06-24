using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskBiDemo.Infrastructure.Data.Configurations;

public sealed class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> builder)
    {
        builder.ToTable("TicketActivities");

        builder.Property(x => x.Description)
            .HasMaxLength(600)
            .IsRequired();

        builder.HasIndex(x => new { x.TicketId, x.CreatedAtUtc });

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActorPerson)
            .WithMany()
            .HasForeignKey(x => x.ActorPersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

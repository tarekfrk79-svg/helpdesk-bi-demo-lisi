using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskBiDemo.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.Property(x => x.Title)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(700)
            .IsRequired();

        builder.Property(x => x.ActionUrl)
            .HasMaxLength(260)
            .IsRequired();

        builder.HasIndex(x => new { x.CompanyId, x.RecipientRole, x.IsRead, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.RecipientPersonId, x.IsRead, x.CreatedAtUtc });
        builder.HasIndex(x => x.TicketId);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RecipientPerson)
            .WithMany()
            .HasForeignKey(x => x.RecipientPersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

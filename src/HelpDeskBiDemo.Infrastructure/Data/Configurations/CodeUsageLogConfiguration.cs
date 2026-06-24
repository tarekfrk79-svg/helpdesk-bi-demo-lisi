using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskBiDemo.Infrastructure.Data.Configurations;

public sealed class CodeUsageLogConfiguration : IEntityTypeConfiguration<CodeUsageLog>
{
    public void Configure(EntityTypeBuilder<CodeUsageLog> builder)
    {
        builder.ToTable("CodeUsageLogs");

        builder.Property(x => x.Source)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasOne(x => x.CompanyAccessCode)
            .WithMany(x => x.UsageLogs)
            .HasForeignKey(x => x.CompanyAccessCodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

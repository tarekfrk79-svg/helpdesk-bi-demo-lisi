using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskBiDemo.Infrastructure.Data.Configurations;

public sealed class CompanyAccessCodeConfiguration : IEntityTypeConfiguration<CompanyAccessCode>
{
    public void Configure(EntityTypeBuilder<CompanyAccessCode> builder)
    {
        builder.ToTable("CompanyAccessCodes");

        builder.Property(x => x.Code)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne(x => x.Company)
            .WithMany(x => x.AccessCodes)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

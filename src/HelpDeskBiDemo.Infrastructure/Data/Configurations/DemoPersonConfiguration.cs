using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskBiDemo.Infrastructure.Data.Configurations;

public sealed class DemoPersonConfiguration : IEntityTypeConfiguration<DemoPerson>
{
    public void Configure(EntityTypeBuilder<DemoPerson> builder)
    {
        builder.ToTable("DemoPeople");

        builder.Property(x => x.FullName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.JobTitle)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Department)
            .HasMaxLength(80);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.People)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

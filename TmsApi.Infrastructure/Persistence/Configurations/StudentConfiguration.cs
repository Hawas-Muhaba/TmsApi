using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        // Natural key: human-readable, must be unique, but never used as a foreign-key target
        builder.HasIndex(s => s.RegistrationNumber).IsUnique();
        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);

        builder.Property<DateTime>("LastUpdated");

        builder.Property(s => s.Version).IsRowVersion();

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

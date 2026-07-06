using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.GPA)
            .HasPrecision(4, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property<DateTime>("LastUpdated");
        builder.Property(s=> s.Version).IsRowVersion();
        // AddHasQueryFilter on Student for !IsDeleted (if not already).
        builder.HasQueryFilter(s => s.IsDeleted == false);

        // builder.IgnoreQueryFilter(s => s.IsDeleted == true);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmaApi.Data.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a=>a.Title).IsRequired().HasMaxLength(200);
        builder.HasOne(a=> a.Course)
            .WithMany(c=>c.Assessments)
            .HasForeignKey(a=>a.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using Microsoft.EntityFrameworkCore;

public sealed class StudentsDbContext : DbContext
{
    public StudentsDbContext(DbContextOptions<StudentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<StudentEntity> Students => Set<StudentEntity>();
}

public sealed class StudentEntity
{
    public string Id { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

using Microsoft.EntityFrameworkCore;

public interface IStudentService
{
    Task<StudentRecord> CreateAsync(string firstName, string lastName, string email);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<StudentRecord?> UpdateAsync(string id, string firstName, string lastName, string email);
    Task<PagedResult<StudentRecord>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<bool> DeleteAsync(string id);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public class StudentService : IStudentService
{
    private readonly StudentsDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(StudentsDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StudentRecord> CreateAsync(string firstName, string lastName, string email)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var now = DateTime.UtcNow;
        var entity = new StudentEntity
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created student {StudentId}", id);
        return ToRecord(entity);
    }

    public async Task<StudentRecord?> GetByIdAsync(string id)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Student {StudentId} not found", id);
            return null;
        }

        return ToRecord(entity);
    }

    public async Task<StudentRecord?> UpdateAsync(string id, string firstName, string lastName, string email)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Student {StudentId} not found for update", id);
            return null;
        }

        entity.FirstName = firstName;
        entity.LastName = lastName;
        entity.Email = email;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated student {StudentId}", id);
        return ToRecord(entity);
    }

    public async Task<PagedResult<StudentRecord>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _context.Students.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await _context.Students
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToRecordExpression())
            .ToListAsync();

        return new PagedResult<StudentRecord>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Delete failed: student {StudentId} not found", id);
            return false;
        }

        _context.Students.Remove(entity);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted student {StudentId}", id);
        return true;
    }

    private static StudentRecord ToRecord(StudentEntity entity)
        => new(entity.Id, entity.FirstName, entity.LastName, entity.Email, entity.CreatedAt, entity.UpdatedAt);

    private static Expression<Func<StudentEntity, StudentRecord>> ToRecordExpression()
        => entity => new StudentRecord(entity.Id, entity.FirstName, entity.LastName, entity.Email, entity.CreatedAt, entity.UpdatedAt);
}

public record StudentRecord(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt,
    DateTime UpdatedAt
);


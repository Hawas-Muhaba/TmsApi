using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, string description);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<CourseRecord?> UpdateAsync(string code, string title, string description);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(TmsDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CourseRecord> CreateAsync(string code, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Course code is required", nameof(code));
        }

        if (await _context.Courses.AnyAsync(c => c.Code == code))
        {
            _logger.LogWarning("Duplicate course create attempt {CourseCode} (record exists)", code);
            var existing = await _context.Courses.AsNoTracking().FirstAsync(c => c.Code == code);
            return ToRecord(existing);
        }

        var course = new Course
        {
            Code = code,
            Title = title,
            Capacity = 30
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created course {CourseCode}", code);
        return ToRecord(course);
    }

    public async Task<CourseRecord?> GetByCodeAsync(string code)
    {
        var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);
        if (course is null)
        {
            _logger.LogWarning("Course {CourseCode} not found", code);
            return null;
        }

        return ToRecord(course);
    }

    public async Task<CourseRecord?> UpdateAsync(string code, string title, string description)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Code == code);
        if (course is null)
        {
            _logger.LogWarning("Course {CourseCode} not found for update", code);
            return null;
        }

        course.Title = title;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated course {CourseCode}", code);
        return ToRecord(course);
    }

    public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        var courses = await _context.Courses.AsNoTracking().OrderBy(c => c.Code).ToListAsync();
        return courses.Select(ToRecord).ToList();
    }

    public async Task<bool> DeleteAsync(string code)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Code == code);
        if (course is null)
        {
            _logger.LogWarning("Delete failed: course {CourseCode} not found", code);
            return false;
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted course {CourseCode}", code);
        return true;
    }

    private static CourseRecord ToRecord(Course course)
        => new(course.Code, course.Title, string.Empty, DateTime.UtcNow, DateTime.UtcNow);
}

public record CourseRecord(
    string Code,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);




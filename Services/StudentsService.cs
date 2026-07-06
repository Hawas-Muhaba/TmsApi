using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public interface IStudentService
{
    Task<Student> CreateAsync(string registrationNumber, string name, decimal gpa, bool isActive = true);
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> UpdateAsync(int id, string registrationNumber, string name, decimal gpa, bool isActive);
    Task<PagedResult<Student>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<bool> DeleteAsync(int id);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public class StudentService : IStudentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(TmsDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Student> CreateAsync(string registrationNumber, string name, decimal gpa, bool isActive = true)
    {
        var student = new Student
        {
            RegistrationNumber = registrationNumber,
            Name = name,
            GPA = gpa,
            IsActive = isActive
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created student {StudentId}", student.Id);
        return student;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null)
        {
            _logger.LogWarning("Student {StudentId} not found", id);
        }

        return student;
    }

    public async Task<Student?> UpdateAsync(int id, string registrationNumber, string name, decimal gpa, bool isActive)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null)
        {
            _logger.LogWarning("Student {StudentId} not found for update", id);
            return null;
        }

        student.RegistrationNumber = registrationNumber;
        student.Name = name;
        student.GPA = gpa;
        student.IsActive = isActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated student {StudentId}", id);
        return student;
    }

    public async Task<PagedResult<Student>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _context.Students.CountAsync();
        var items = await _context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<Student>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null)
        {
            _logger.LogWarning("Delete failed: student {StudentId} not found", id);
            return false;
        }
        
        
        // softdelete
        student.IsDeleted = true;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted student {StudentId}", id);
        return true;

        // _context.Students.Remove(student);
        // await _context.SaveChangesAsync();
        // _logger.LogInformation("Deleted student {StudentId}", id);
        // return true;
    }

    public async Task<IEnumerable<dynamic>> GetStudnetReportAsync()
    {
        var report = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync();
            return report;
    }
    
        // Set default SQL or update logic e.g. set shadow property before SaveChanges in your service layer:
    public async Task UpdateLastUpdatedAsync(Student student)
    {
        _context.Entry(student).Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    } 
}


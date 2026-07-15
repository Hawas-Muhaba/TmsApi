using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;
public class CourseService(TmsDbContext context, ILogger<CourseService> logger): ICourseService
{
    public async Task<CourseResponseDto?> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);
        return await GetByIdAsync(course.Id, ct);
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses.AsNoTracking().Where(c=>c.Id == id)
                        .Select(c=> new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
                        .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        var courses = await context.Courses.AsNoTracking().ToListAsync();
        var records = courses
            .Select(c => new CourseRecord(c.Id.ToString(), c.Code, c.Title, 0, DateTime.UtcNow))
            .ToList();
        return records;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var courseId))
        {
            logger.LogWarning("Invalid course ID format {CourseId}", id);
            return false;
        }

        var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        
        if (course is null)
        {
            logger.LogWarning("Delete failed course {CourseId} not found", id);
            return false;
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted course {CourseId}", id);
        return true;
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        await context.Courses.AsNoTracking().AnyAsync(c=>c.Code == code, ct);
}
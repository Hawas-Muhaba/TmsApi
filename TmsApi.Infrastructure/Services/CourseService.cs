using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

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

    public async Task<CourseResponseDto?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await context.Courses.AsNoTracking()
            .Where(c => c.Code == code)
            .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }

    // public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    // {
    //     var courses = await context.Courses.AsNoTracking().ToListAsync();
    //     var records = courses
    //         .Select(c => new CourseRecord(c.Id.ToString(), c.Code, c.Title, 0, DateTime.UtcNow))
    //         .ToList();
    //     return records;
    // }
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
PagedRequest request, CancellationToken ct)
    {
        IQueryable<Course> query = context.Courses.AsNoTracking();
        if(query is null)
        {
            logger.LogWarning("GetCoursesAsync query is null");
            return new PagedResponse<CourseResponseDto>
            {
                Items = new List<CourseResponseDto>(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
        query = query.Where(c => EF.Functions.ILike(c.Title, $"%{request.Search}%"));
        var totalCount = await query.CountAsync(ct);

//         // TODO 4: Apply OrderBy, then Skip/Take, then Select projection.
// // For OrderBy, branch on request.OrderBy ∈ { "Title", "Code", "
// MaxCapacity" }
// // and apply Descending if request.Descending. Reject unknown Ord
// erBy values
// // silently by falling back to "Title" never let an arbitrary st
// ring
// // into the LINQ tree.
        query = request.OrderBy switch
        {
            "Code" => request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "MaxCapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
        };

        var courses = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = courses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
        throw new NotImplementedException();
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
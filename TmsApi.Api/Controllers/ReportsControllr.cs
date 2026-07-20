using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
[Tags("Reports")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ReportsController(TmsDbContext context): ControllerBase
{
    [HttpGet("students")]
    [ProducesResponseType(typeof(IReadOnlyList<Student>), StatusCodes.Status200OK)]
    [EndpointSummary("Get paged students")]
    [EndpointDescription("Returns a paginated list of students ordered by name.")]
    public async Task<IActionResult> GetStudentsPaged(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page-1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            return Ok(students);
    }

    //N+1 fix, one shaped query
    [HttpGet("enrollment-counts")]
    [ProducesResponseType(typeof(IReadOnlyList<object>), StatusCodes.Status200OK)]
    [EndpointSummary("Get enrollment counts per student")]
    [EndpointDescription("Returns student names with their enrollment counts.")]
    public async Task<IActionResult> GetEnrollmentCounts(CancellationToken ct = default)
    {
        var report = await context.Students
            .AsNoTracking()
            .Select(s => new { s.Name, EnrollmentCount = s.Enrollments.Count })
            .ToListAsync(ct);

        return Ok(report);
    }

    //bulk archive
    [HttpPost("archive-enrollments")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [EndpointSummary("Archive old enrollments")]
    [EndpointDescription("Archives enrollments older than the provided date.")]
    public async Task<IActionResult> ArchiveOldEnrollments([FromQuery] DateTime olderThan, CancellationToken ct = default)
    {
        var affected = await context.Enrollments
            .Where(e=>e.EnrolledAt <olderThan && !e.IsArchived)
            .ExecuteUpdateAsync(s=> s.SetProperty(e=>e.IsArchived, true), ct);
        
        return Ok(new {archivedCount = affected});
    }
    //soft-delete restore for admin only
    [HttpGet("students/deleted")]
    [ProducesResponseType(typeof(IReadOnlyList<Student>), StatusCodes.Status200OK)]
    [EndpointSummary("Get deleted students")]
    [EndpointDescription("Returns students that have been soft-deleted.")]
    public async Task<IActionResult> GetDeletedStudents(CancellationToken ct = default)
    {
        var deleted = await context.Students
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .ToListAsync(ct);

        return Ok(deleted);
    }

    [HttpPost("students/active-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [EndpointSummary("Get active students count")]
    [EndpointDescription("Returns the count of active students with GPA >= 3.0.")]
    public async Task<IActionResult> GetActiveStudentsCount(CancellationToken ct = default)
    {
        var count = await context.Students
            .Where(s=> s.IsActive && s.GPA >= 3.0m)
            .CountAsync();
    
        return Ok(new { count });
    }

    [HttpGet("courses/enrollment-counts")]
    [ProducesResponseType(typeof(IReadOnlyList<object>), StatusCodes.Status200OK)]
    [EndpointSummary("Get course enrollment counts")]
    [EndpointDescription("Returns course titles with their enrollment counts.")]
    public async Task<IActionResult> GetCoursesEnrollmentCounts(CancellationToken ct = default)
    {
        var report = await context.Courses
            .Select(c => new {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync(ct);

        return Ok(report);
    }

    //what is the average gpa per course
    [HttpGet("courses/average-gpa")]
    [ProducesResponseType(typeof(IReadOnlyList<object>), StatusCodes.Status200OK)]
    [EndpointSummary("Get average GPA per course")]
    [EndpointDescription("Returns average GPA for each course.")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e=>e.Course.Title)
            .Select( g => new {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }
    
    [HttpGet("students/no-enrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [EndpointSummary("Get students without enrollments")]
    [EndpointDescription("Returns the names of students who are not enrolled in any course.")]
    public async Task<IActionResult> GetStudentsWithNoEnrollments()
    {
        var list = await context.Students
            .Where( s => !s.Enrollments.Any())
            .Select(s=> s.Name)
            .ToListAsync();
        return Ok(list);
    }
}

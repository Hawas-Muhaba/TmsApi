using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]

public class ReportsController(TmsDbContext context): ControllerBase
{
    [HttpGet("students")]
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
    public async Task<IActionResult> ArchiveOldEnrollments([FromQuery] DateTime olderThan, CancellationToken ct = default)
    {
        var affected = await context.Enrollments
            .Where(e=>e.EnrolledAt <olderThan && !e.IsArchived)
            .ExecuteUpdateAsync(s=> s.SetProperty(e=>e.IsArchived, true), ct);
        
        return Ok(new {archivedCount = affected});
    }
    //soft-delete restore for admin only
    [HttpGet("students/deleted")]
    public async Task<IActionResult> GetDeletedStudents(CancellationToken ct = default)
    {
        var deleted = await context.Students
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .ToListAsync(ct);

        return Ok(deleted);
    }

}
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService, IReportingService reportingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("top-enrolled")]
    public async Task<IActionResult> GetTopEnrolledCourses([FromQuery] int take = 5)
    {
        var summary = await reportingService.GetTopCoursesByEnrollmentAsync(take);
        return Ok(summary);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var course = await courseService.GetByCodeAsync(code);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var course = await courseService.CreateAsync(request.Code, request.Title, request.Description);
        return CreatedAtAction(nameof(GetByCode), new { code = course.Code }, course);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateCourseRequest request)
    {
        var updated = await courseService.UpdateAsync(code, request.Title, request.Description);
        return updated is not null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateCourseRequest(string Code, string Title, string Description);
public record UpdateCourseRequest(string Title, string Description);

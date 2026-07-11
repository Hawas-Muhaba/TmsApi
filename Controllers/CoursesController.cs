using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
// GET /api/courses
[HttpGet]
public async Task<IActionResult> GetAll()
    => Ok(await courseService.GetAllAsync());

// GET /api/courses/{id}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id)
{
    var record = await courseService.GetByIdAsync(id);
    return record is not null ? Ok(record) : NotFound();
}

// POST /api/courses -> 201 + Location
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
{
    var record = await courseService.CreateAsync(request.Code, request.Title, request.Credits);
    return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
}

// DELETE /api/courses/{id} -> 204 or 404
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
    var deleted = await courseService.DeleteAsync(id);
    return deleted ? NoContent() : NotFound();
}
}

public record CreateCourseRequest(string Code, string Title, int Credits);
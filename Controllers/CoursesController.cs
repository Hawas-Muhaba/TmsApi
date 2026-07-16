using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;
[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
// GET /api/courses
// [HttpGet]
// public async Task<IActionResult> GetAll()
//     => Ok(await courseService.GetAllAsync());

[HttpGet]
public async Task<IActionResult> GetCourses(
[FromQuery] PagedRequest request, CancellationToken ct)
{
var result = await courseService.GetCoursesAsync(request, ct);
return Ok(result);
}
// GET /api/courses/{id}
[HttpGet("{id:int}", Name = nameof(GetCourseById))]
public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
{
    var course = await courseService.GetByIdAsync(id, ct);
    return course is not null ? Ok(course) : NotFound();
}

// POST /api/courses -> 201 + Location
[HttpPost]
public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
{
    var exists = await courseService.CodeExistsAsync(request.Code, ct);

    if(exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }
    var result = await courseService.CreateAsync(request, ct);
    return CreatedAtAction(nameof(GetCourseById), new {id = result.Id}, result);
}

// DELETE /api/courses/{id} -> 204 or 404
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
    var deleted = await courseService.DeleteAsync(id);
    return deleted ? NoContent() : NotFound();
}
}

using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription("Returns paged students with optional search and ordering.")]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await studentService.GetStudentsAsync(request, ct));

    [HttpGet("{id}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get student by ID")]
    [EndpointDescription("Returns the specified student or 404 if not found.")]
    public async Task<IActionResult> GetStudentById(string id)
    {
        var record = await studentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new student")]
    [EndpointDescription("Creates a student with a unique name. Returns 409 if a student with the same name already exists.")]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken ct)
    {
        if (await studentService.ExistsAsync(request.Name, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Student already exists",
                Detail = $"A student named '{request.Name}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var record = await studentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetStudentById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Deletes the specified student if they exist.")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
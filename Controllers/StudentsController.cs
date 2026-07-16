using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await studentService.GetStudentsAsync(request, ct));

    [HttpGet("{id}", Name = nameof(GetStudentById))]
    public async Task<IActionResult> GetStudentById(string id)
    {
        var record = await studentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
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
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
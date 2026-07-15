using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await studentService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await studentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken ct)
    {
        var record = await studentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
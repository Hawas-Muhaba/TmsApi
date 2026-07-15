using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var record = await enrollmentService.GetByIdAsync(id, ct);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request, CancellationToken ct)
    {
        var record = await enrollmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/assessments")]
public class AssessmentsController(IAssessmentService assessmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await assessmentService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await assessmentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request, CancellationToken ct)
    {
        var record = await assessmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await assessmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("results")]
    [Authorize]
    public IActionResult GetResults() => Ok(new {
        courseCode = "CS-101", studentId = "S-001", letterGrade = "A"
    });
}
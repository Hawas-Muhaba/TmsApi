using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AssessmentsController(IAssessmentService assessmentService) : ControllerBase
{
[HttpGet]
public async Task<IActionResult> GetAll()
    => Ok(await assessmentService.GetAllAsync());

// GET /api/assessments/{id}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id)
{
    var record = await assessmentService.GetByIdAsync(id);
    return record is not null ? Ok(record) : NotFound();
}
//POST /api/assessments -> 2001 + location
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request)
{
    var record = await assessmentService.RecordAsync(
        request.StudentId, request.CourseCode, request.Score, request.MaxScore
    );
    return CreatedAtAction(nameof(GetById), new{id = record.Id}, record);
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


public record CreateAssessmentRequest(string StudentId, string CourseCode, decimal Score, decimal MaxScore);
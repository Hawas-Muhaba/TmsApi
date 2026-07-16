using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/assessments")]
[Tags("Assessments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AssessmentsController(IAssessmentService assessmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List assessments with pagination")]
    [EndpointDescription("Returns paginated assessments with optional search and ordering.")]
    public async Task<IActionResult> GetAssessments([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await assessmentService.GetAssessmentsAsync(request, ct));

    [HttpGet("{id}", Name = nameof(GetAssessmentById))]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get assessment by ID")]
    [EndpointDescription("Returns the specified assessment or 404 if it does not exist.")]
    public async Task<IActionResult> GetAssessmentById(string id)
    {
        var record = await assessmentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new assessment")]
    [EndpointDescription("Creates a course assessment; returns 409 if the assessment already exists for the target course.")]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request, CancellationToken ct)
    {
        if (await assessmentService.ExistsAsync(request.Title, request.CourseId, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Assessment already exists",
                Detail = $"An assessment with title '{request.Title}' already exists for course {request.CourseId}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var record = await assessmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAssessmentById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an assessment")]
    [EndpointDescription("Deletes the specified assessment if it exists.")]
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
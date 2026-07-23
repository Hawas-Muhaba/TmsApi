using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Assessments.Commands;
using TmsApi.Application.Assessments.Queries;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Api.Controllers;

[ApiController]
[EnableRateLimiting("assessments")]
[Route("api/v{version:apiVersion}/assessments")]
[ApiVersion("2.0")]
[Tags("Assessments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AssessmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List assessments with pagination")]
    [EndpointDescription("Returns paginated assessments with optional search and ordering.")]
    public async Task<IActionResult> GetAssessments([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var query = new GetAssessmentsQuery(request);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id}", Name = nameof(GetAssessmentById))]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get assessment by ID")]
    [EndpointDescription("Returns the specified assessment or 404 if it does not exist.")]
    public async Task<IActionResult> GetAssessmentById(string id, CancellationToken ct)
    {
        var query = new GetAssessmentQuery(id);
        var result = await mediator.Send(query, ct);
        
        return result.Match<IActionResult>(
            onSuccess: assessment => Ok(assessment),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Assessment not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new assessment")]
    [EndpointDescription("Creates a course assessment; returns 409 if the assessment already exists for the target course.")]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request, CancellationToken ct)
    {
        var command = new CreateAssessmentCommand(request.Title, request.MaxScore, request.Weight, request.CourseId);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(nameof(GetAssessmentById), new { id = created.Id }, created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "assessment_exists" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };
                return Problem(statusCode: status, title: "Assessment creation failed",
                    detail: error.Message, type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an assessment")]
    [EndpointDescription("Deletes the specified assessment if it exists.")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var command = new DeleteAssessmentCommand(id);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: _ => NoContent(),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Assessment not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }

    [HttpGet("results")]
    [Authorize]
    public IActionResult GetResults() => Ok(new {
        courseCode = "CS-101", studentId = "S-001", letterGrade = "A"
    });
}
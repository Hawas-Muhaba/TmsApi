using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;
using TmsApi.Application.Students.Commands;
using TmsApi.Application.Students.Queries;

namespace TmsApi.Api.Controllers;

[ApiController]
[EnableRateLimiting("students")]
[Route("api/v{version:apiVersion}/students")]
[ApiVersion("2.0")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription("Returns paged students with optional search and ordering.")]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var query = new GetStudentsQuery(request);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get student by ID")]
    [EndpointDescription("Returns the specified student or 404 if not found.")]
    public async Task<IActionResult> GetStudentById(string id, CancellationToken ct)
    {
        var query = new GetStudentQuery(id);
        var result = await mediator.Send(query, ct);
        
        return result.Match<IActionResult>(
            onSuccess: student => Ok(student),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Student not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new student")]
    [EndpointDescription("Creates a student with a unique name. Returns 409 if a student with the same name already exists.")]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken ct)
    {
        var command = new CreateStudentCommand(request.Name);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(nameof(GetStudentById), new { id = created.Id }, created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "name_exists" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };
                return Problem(statusCode: status, title: "Student creation failed",
                    detail: error.Message, type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Deletes the specified student if they exist.")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var command = new DeleteStudentCommand(id);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: _ => NoContent(),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Student not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }
}
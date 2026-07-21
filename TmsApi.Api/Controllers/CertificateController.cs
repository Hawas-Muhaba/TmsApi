using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Certificates.Commands;
using TmsApi.Application.Certificates.Queries;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/certificates")]
[ApiVersion("2.0")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CertificateResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List certificates with pagination")]
    [EndpointDescription("Returns paged certificates with optional search and ordering.")]
    public async Task<IActionResult> GetCertificates([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var query = new GetCertificatesQuery(request);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id}", Name = nameof(GetCertificateById))]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get certificate by ID")]
    [EndpointDescription("Returns the specified certificate or 404 if not found.")]
    public async Task<IActionResult> GetCertificateById(string id, CancellationToken ct)
    {
        var query = new GetCertificateQuery(id);
        var result = await mediator.Send(query, ct);
        
        return result.Match<IActionResult>(
            onSuccess: cert => Ok(cert),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Certificate not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new certificate")]
    [EndpointDescription("Issues a certificate when the student is not already certified for the course.")]
    public async Task<IActionResult> Create([FromBody] CreateCertificateRequest request, CancellationToken ct)
    {
        var command = new CreateCertificateCommand(request.StudentId, request.CourseId);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(nameof(GetCertificateById), new { id = created.Id }, created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "certificate_exists" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };
                return Problem(statusCode: status, title: "Certificate creation failed",
                    detail: error.Message, type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a certificate")]
    [EndpointDescription("Deletes the specified certificate if it exists.")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var command = new DeleteCertificateCommand(id);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: _ => NoContent(),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Certificate not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }
}
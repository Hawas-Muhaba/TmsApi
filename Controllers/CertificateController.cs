using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(ICertificateService certificateService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CertificateResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List certificates with pagination")]
    [EndpointDescription("Returns paged certificates with optional search and ordering.")]
    public async Task<IActionResult> GetCertificates([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await certificateService.GetCertificatesAsync(request, ct));

    [HttpGet("{id}", Name = nameof(GetCertificateById))]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get certificate by ID")]
    [EndpointDescription("Returns the specified certificate or 404 if not found.")]
    public async Task<IActionResult> GetCertificateById(string id)
    {
        var record = await certificateService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new certificate")]
    [EndpointDescription("Issues a certificate when the student is not already certified for the course.")]
    public async Task<IActionResult> Create([FromBody] CreateCertificateRequest request, CancellationToken ct)
    {
        if (await certificateService.ExistsAsync(request.StudentId, request.CourseId, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Certificate already exists",
                Detail = $"Student {request.StudentId} already has a certificate for course {request.CourseId}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var record = await certificateService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCertificateById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a certificate")]
    [EndpointDescription("Deletes the specified certificate if it exists.")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await certificateService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
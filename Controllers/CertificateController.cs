using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/certificates")]
public class CertificatesController(ICertificateService certificateService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCertificates([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await certificateService.GetCertificatesAsync(request, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await certificateService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
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
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await certificateService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
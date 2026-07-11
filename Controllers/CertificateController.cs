using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/certificates")]
public class CertificatesController(ICertificateService certificateService) : ControllerBase
{
// GET /api/certificates
[HttpGet]
public async Task<IActionResult> GetAll()
    => Ok(await certificateService.GetAllAsync());

// GET /api/certificates/{id}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id)
{
    var record = await certificateService.GetByIdAsync(id);
    return record is not null ? Ok(record) : NotFound();
}

// POST /api/certificates -> 201 + Location
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateCertificateRequest request)
{
    var record = await certificateService.IssueAsync(request.StudentId, request.CourseCode);
    return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
}

// DELETE /api/certificates/{id} -> 204 or 404
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
    var deleted = await certificateService.DeleteAsync(id);
    return deleted ? NoContent() : NotFound();
}
}

public record CreateCertificateRequest(string StudentId, string CourseCode);
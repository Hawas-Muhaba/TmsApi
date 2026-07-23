using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}")]
public class TranscriptController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TranscriptController> _logger;

    public TranscriptController(IMediator mediator, ILogger<TranscriptController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Request a transcript for a session or enrollment
    /// Returns 202 Accepted with Location header pointing to status endpoint
    /// </summary>
    [HttpPost("transcripts")]
    [EnableRateLimiting("transcripts")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] RequestTranscriptDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transcript request received for session/enrollment: {SessionId}", request.SessionId);

        // Process asynchronously in background
        // This is a fire-and-forget pattern with 202 Accepted response
        _ = Task.Run(async () =>
        {
            try
            {
                // Placeholder for actual transcript generation logic
                _logger.LogInformation("Processing transcript generation for session: {SessionId}", request.SessionId);
                await Task.Delay(1000); // Simulated processing
                _logger.LogInformation("Transcript generation completed for session: {SessionId}", request.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating transcript for session: {SessionId}", request.SessionId);
            }
        }, cancellationToken);

        // Return 202 Accepted with Location header
        var statusUrl = Url.Action(
            nameof(GetTranscriptStatus),
            "Transcript",
            new { sessionId = request.SessionId },
            Request.Scheme);

        return Accepted(statusUrl, new { message = "Transcript generation started" });
    }

    /// <summary>
    /// Check status of transcript generation
    /// </summary>
    [HttpGet("transcripts/{sessionId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTranscriptStatus(string sessionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking transcript status for session: {SessionId}", sessionId);

        // Placeholder: In production, query database for transcript status
        return Ok(new
        {
            sessionId = sessionId,
            status = "completed",
            generatedAt = DateTime.UtcNow
        });
    }
}

public class RequestTranscriptDto
{
    public string SessionId { get; set; } = null!;
    public string? EnrollmentId { get; set; }
    public string? Format { get; set; } = "json";
}

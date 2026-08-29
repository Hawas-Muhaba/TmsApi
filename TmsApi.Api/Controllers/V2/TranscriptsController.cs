using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v2/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) : ControllerBase
{
    private static readonly SemaphoreSlim idempotencyGate = new(1, 1);

    [HttpPost]
    [EnableRateLimiting("transcripts")]
    [ProducesResponseType(typeof(TranscriptStatus), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        await idempotencyGate.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingReportId = await statusStore.GetReportIdForIdempotencyKeyAsync(idempotencyKey, ct);
                if (existingReportId is not null)
                {
                    var existingStatus = await statusStore.GetAsync(existingReportId, ct);
                    return Accepted(GetStatusUrl(existingReportId), existingStatus);
                }
            }

            var reportId = Guid.NewGuid().ToString("N")[..12];
            var status = await statusStore.CreateAsync(reportId, request.StudentId, ct);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                await statusStore.LinkIdempotencyKeyAsync(idempotencyKey, reportId, ct);

            await channel.Writer.WriteAsync(request.WithReportId(reportId), ct);
            Response.Headers.RetryAfter = "5";
            return Accepted(GetStatusUrl(reportId), status);
        }
        finally
        {
            idempotencyGate.Release();
        }
    }

    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(TranscriptStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(string id, CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);
        return status is null
            ? NotFound(new ProblemDetails
            {
                Title = "Transcript not found",
                Detail = $"No transcript request with id '{id}'.",
                Status = StatusCodes.Status404NotFound
            })
            : Ok(status);
    }

    private string GetStatusUrl(string reportId) =>
        Url.Action(nameof(GetStatus), new { id = reportId })
        ?? $"/api/v2/transcripts/{reportId}/status";
}

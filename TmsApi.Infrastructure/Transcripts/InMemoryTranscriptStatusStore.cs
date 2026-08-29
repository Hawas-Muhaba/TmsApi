using System.Collections.Concurrent;
using TmsApi.Application.Transcripts;

namespace TmsApi.Infrastructure.Transcripts;

public class InMemoryTranscriptStatusStore : ITranscriptStatusStore
{
    private readonly ConcurrentDictionary<string, TranscriptStatus> statuses = new();
    private readonly ConcurrentDictionary<string, string> idempotencyToReportId = new();

    public Task<TranscriptStatus> CreateAsync(string reportId, int studentId, CancellationToken ct)
    {
        var status = new TranscriptStatus(reportId, studentId, TranscriptState.Queued, DateTimeOffset.UtcNow);
        statuses[reportId] = status;
        return Task.FromResult(status);
    }

    public Task MarkProcessingAsync(string reportId, CancellationToken ct) =>
        Transition(reportId, current => current with
        {
            State = TranscriptState.Processing,
            StartedAt = DateTimeOffset.UtcNow
        }, TranscriptState.Queued);

    public Task MarkReadyAsync(string reportId, string downloadUrl, CancellationToken ct) =>
        Transition(reportId, current => current with
        {
            State = TranscriptState.Ready,
            CompletedAt = DateTimeOffset.UtcNow,
            DownloadUrl = downloadUrl
        }, TranscriptState.Processing);

    public Task MarkFailedAsync(string reportId, string error, CancellationToken ct) =>
        Transition(reportId, current => current with
        {
            State = TranscriptState.Failed,
            CompletedAt = DateTimeOffset.UtcNow,
            ErrorMessage = error
        }, TranscriptState.Processing);

    public Task<TranscriptStatus?> GetAsync(string reportId, CancellationToken ct) =>
        Task.FromResult(statuses.TryGetValue(reportId, out var status) ? status : null);

    public Task<string?> GetReportIdForIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct) =>
        Task.FromResult(idempotencyToReportId.TryGetValue(idempotencyKey, out var reportId) ? reportId : null);

    public Task LinkIdempotencyKeyAsync(string idempotencyKey, string reportId, CancellationToken ct)
    {
        idempotencyToReportId.TryAdd(idempotencyKey, reportId);
        return Task.CompletedTask;
    }

    private Task Transition(
        string reportId,
        Func<TranscriptStatus, TranscriptStatus> change,
        TranscriptState allowedFrom)
    {
        while (true)
        {
            if (!statuses.TryGetValue(reportId, out var current))
                throw new InvalidOperationException($"Unknown report id {reportId}.");
            if (current.State != allowedFrom)
                throw new InvalidOperationException(
                    $"Cannot move {reportId} from {current.State} via this transition (expected {allowedFrom}).");
            if (statuses.TryUpdate(reportId, change(current), current))
                return Task.CompletedTask;
        }
    }
}

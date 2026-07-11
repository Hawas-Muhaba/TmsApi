public class CertificateService : ICertificateService
{
    private readonly Dictionary<string, CertificateRecord> _store = new();
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(ILogger<CertificateService> logger)
    {
        _logger = logger;
    }

    public Task<CertificateRecord> IssueAsync(string studentId, string courseCode)
    {
        // A student should not receive two certificates for the same course
        var existing = _store.Values
            .FirstOrDefault(c => c.StudentId == studentId && c.CourseCode == courseCode);

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate certificate issue attempt {StudentId} already certified for {CourseCode} (record {CertificateId})",
                studentId, courseCode, existing.Id);
            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var certificateNumber = $"CERT-{DateTime.UtcNow:yyyy}-{id.ToUpperInvariant()}";
        var record = new CertificateRecord(id, studentId, courseCode, certificateNumber, DateTime.UtcNow);
        _store[id] = record;

        _logger.LogInformation(
            "Issued certificate {CertificateNumber} to {StudentId} for {CourseCode} record {CertificateId}",
            certificateNumber, studentId, courseCode, id);

        return Task.FromResult(record);
    }

    public Task<CertificateRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);

        if (record is null)
        {
            _logger.LogWarning("Certificate {CertificateId} not found", id);
        }

        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<CertificateRecord>> GetAllAsync()
    {
        IReadOnlyList<CertificateRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        if (removed)
            _logger.LogInformation("Deleted certificate {CertificateId}", id);
        else
            _logger.LogWarning("Delete failed certificate {CertificateId} not found", id);

        return Task.FromResult(removed);
    }
}

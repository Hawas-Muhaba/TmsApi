
public class StudentService : IStudentService
{
    private readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<StudentRecord> CreateAsync(string fullName, string email)
    {
        var existing = _store.Values.FirstOrDefault(s=> s.Email == email);

        if(existing is not null)
        {
            _logger.LogWarning("Duplicate student registration attempt {Email} (record {StudentId})",
            email, existing.Id);
            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new StudentRecord(id, fullName, email, DateTime.UtcNow);

        _store[id] = record;
        _logger.LogInformation("Created student {StudentId} {FullName} {Email}",
        id, fullName, email);

        return Task.FromResult(record);
    }

    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);

        if(record is null)
        {
            _logger.LogWarning("Student {StudentId} not found", id);
        }
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        var records = _store.Values.ToList();
        _logger.LogInformation("Retrieved {Count} students", records.Count);
        return Task.FromResult<IReadOnlyList<StudentRecord>>(records);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        if(removed)
            _logger.LogInformation("deleted student {StudentId}", id);
        else
            _logger.LogWarning("Delete failed student {StudentId} not found", id);
        return Task.FromResult(removed);
    }

}

using Microsoft.AspNetCore.Razor.TagHelpers;

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, string description);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<CourseRecord?> UpdateAsync(string code, string title, string description);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private readonly Dictionary<string, CourseRecord> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<CourseRecord> CreateAsync(string code, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Course code is required", nameof(code));
        }

        if (_store.TryGetValue(code, out var existing))
        {
            _logger.LogWarning("Duplicate course create attempt {CourseCode} (record exists)", code);
            return Task.FromResult(existing);
        }

        var record = new CourseRecord(code, title, description, DateTime.UtcNow, DateTime.UtcNow);
        _store[code] = record;
        _logger.LogInformation("Created course {CourseCode}", code);
        return Task.FromResult(record);
    }

    public Task<CourseRecord?> GetByCodeAsync(string code)
    {
        _store.TryGetValue(code, out var record);
        if (record is null)
        {
            _logger.LogWarning("Course {CourseCode} not found", code);
        }
        return Task.FromResult(record);
    }

    public Task<CourseRecord?> UpdateAsync(string code, string title, string description)
    {
        if (!_store.TryGetValue(code, out var existing))
        {
            _logger.LogWarning("Course {CourseCode} not found for update", code);
            return Task.FromResult<CourseRecord?>(null);
        }

        var updated = existing with
        {
            Title = title,
            Description = description,
            UpdatedAt = DateTime.UtcNow
        };

        _store[code] = updated;
        _logger.LogInformation("Updated course {CourseCode}", code);
        return Task.FromResult<CourseRecord?>(updated);
    }

    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        IReadOnlyList<CourseRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string code)
    {
        var removed = _store.Remove(code);
        if (removed)
        {
            _logger.LogInformation("Deleted course {CourseCode}", code);
        }
        else
        {
            _logger.LogWarning("Delete failed: course {CourseCode} not found", code);
        }

        return Task.FromResult(removed);
    }
}

public record CourseRecord(
    string Code,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);




public class AssessmentService: IAssessmentService
{
    private readonly Dictionary<string, AssessmentRecord> _store = new();
    private readonly ILogger<AssessmentService> _logger;
    public AssessmentService(ILogger<AssessmentService> logger)
    {
        _logger = logger;
    }
    public Task<AssessmentRecord> RecordAsync(string studentId, string courseCode, decimal score, decimal maxScore)
    {
        if(score> maxScore)
        {
            _logger.LogWarning("IScore {Score} exceeds max {MaxScore} for {StudentId} in {CourseCode} clamping to max",
            score, maxScore, studentId, courseCode);
            score = maxScore;
        }
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new AssessmentRecord(id, studentId, courseCode, score, maxScore, DateTime.UtcNow);
        _store[id] = record;
         _logger.LogInformation(
            "Recorded assessment {StudentId} {CourseCode} {Score}/{MaxScore} record {AssessmentId}",
            studentId, courseCode, score, maxScore, id);

        return Task.FromResult(record);
    }
    public Task<AssessmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        if(record is null)
        {
            _logger.LogWarning("Assessment {AssessmentId} not found", id);
        }
        return Task.FromResult(record);
    }
    public Task<IReadOnlyList<AssessmentRecord>> GetAllAsync()
    {
        IReadOnlyList<AssessmentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if(removed)
          _logger.LogInformation("Deleted assessment {AssessmentId}", id);
        else
            _logger.LogWarning("Delete failed assessment {AssessmentId} not found", id);

        return Task.FromResult(removed);
    }
}
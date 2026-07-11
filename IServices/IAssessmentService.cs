public interface IAssessmentService
{
    Task<AssessmentRecord> RecordAsync(string studentId, string courseCode, decimal score, decimal maxScore);
    Task<AssessmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<AssessmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}


public record AssessmentRecord(
    string Id, string StudentId, string CourseCode, decimal Score, decimal MaxScore, DateTime RecordedAt
);
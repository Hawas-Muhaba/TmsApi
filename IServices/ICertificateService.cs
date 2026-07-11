public interface ICertificateService
{
    Task<CertificateRecord> IssueAsync(string studentId, string courseCode);
    Task<CertificateRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<CertificateRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
public record CertificateRecord(
    string Id, string StudentId, string CourseCode, string CertificateNumber, DateTime IssuedAt);

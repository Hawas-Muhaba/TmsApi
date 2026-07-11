
public interface IStudentService
{
    Task<StudentRecord> CreateAsync (string fullName, string email);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}


public record StudentRecord(string Id, string FullName, string Email, DateTime RegisteredAt);
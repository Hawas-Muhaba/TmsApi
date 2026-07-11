public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, int credits);
    Task<CourseRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public record CourseRecord(string Id, string Code, string Title, int Credits, DateTime CreatedAt);
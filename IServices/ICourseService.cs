
using System.Threading;
using TmsApi.Dtos;
public interface ICourseService
{
    Task<CourseResponseDto?> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

}

public record CourseRecord(string Id, string Code, string Title, int Credits, DateTime CreatedAt);
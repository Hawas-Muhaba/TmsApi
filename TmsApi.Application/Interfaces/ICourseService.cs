
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto?> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(string id);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

}

public interface ICachedCourseService
{
    Task<CourseResponseDto?> GetCourseAsync(string code, CancellationToken ct);
    Task<List<CourseResponseDto>> GetAllCoursesAsync(CancellationToken ct);
    Task InvalidateCourseCacheAsync(CancellationToken ct);
}

public record CourseRecord(string Id, string Code, string Title, int Credits, DateTime CreatedAt,
    int MaxCapacity = 0, int EnrollmentCount = 0);
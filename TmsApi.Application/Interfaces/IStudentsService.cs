using System.Threading;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<bool> ExistsAsync(string name, CancellationToken ct);
    Task<StudentResponseDto?> GetByIdAsync(string id);
    // Task<IReadOnlyList<StudentResponseDto>> GetAllAsync();
    public Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(string id);
}
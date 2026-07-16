using System.Threading;
using TmsApi.Dtos;
using TmsApi.Entities;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(CreateEnrollmentRequest request, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<PagedResponse<EnrollmentResponseDto>> GetEnrollmentsAsync(int courseId, PagedRequest request, CancellationToken ct);
    Task<EnrollmentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

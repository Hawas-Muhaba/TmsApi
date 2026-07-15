using System.Threading;
using TmsApi.Dtos;
using TmsApi.Entities;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(CreateEnrollmentRequest request, CancellationToken ct);
    Task<EnrollmentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

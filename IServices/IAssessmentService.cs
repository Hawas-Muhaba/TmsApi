using System.Threading;
using TmsApi.Dtos;

public interface IAssessmentService
{
    Task<AssessmentResponseDto> CreateAsync(CreateAssessmentRequest request, CancellationToken ct);
    Task<bool> ExistsAsync(string title, int courseId, CancellationToken ct);
    Task<AssessmentResponseDto?> GetByIdAsync(string id);
    Task<IReadOnlyList<AssessmentResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
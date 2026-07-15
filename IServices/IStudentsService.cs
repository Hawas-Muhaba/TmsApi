using System.Threading;
using TmsApi.Dtos;

public interface IStudentService
{
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<StudentResponseDto?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
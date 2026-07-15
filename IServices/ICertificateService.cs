using System.Threading;
using TmsApi.Dtos;

public interface ICertificateService
{
    Task<CertificateResponseDto> CreateAsync(CreateCertificateRequest request, CancellationToken ct);
    Task<CertificateResponseDto?> GetByIdAsync(string id);
    Task<IReadOnlyList<CertificateResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

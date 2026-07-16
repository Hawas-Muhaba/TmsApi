using Microsoft.EntityFrameworkCore;
using System.Threading;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;
namespace TmsApi.Services;

public class CertificateService(TmsDbContext context, ILogger<CertificateService> logger) : ICertificateService
{
    public async Task<CertificateResponseDto> CreateAsync(CreateCertificateRequest request, CancellationToken ct)
    {
        // Check if student already has certificate for this course
        var existing = await context.Certificates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.StudentId == request.StudentId && c.CourseId == request.CourseId, ct);

        if (existing is not null)
        {
            logger.LogWarning(
                "Duplicate certificate issue attempt {StudentId} already certified for course {CourseId} (record {CertificateId})",
                request.StudentId, request.CourseId, existing.Id);
            return new CertificateResponseDto(existing.Id, existing.SerialNumber, existing.IssuedAt, existing.StudentId, existing.CourseId);
        }

        var serialNumber = $"CERT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var certificate = new Certificate
        {
            SerialNumber = serialNumber,
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            IssuedAt = DateTime.UtcNow
        };

        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Issued certificate {SerialNumber} to student {StudentId} for course {CourseId} record {CertificateId}",
            serialNumber, request.StudentId, request.CourseId, certificate.Id);

        return new CertificateResponseDto(certificate.Id, certificate.SerialNumber, certificate.IssuedAt, certificate.StudentId, certificate.CourseId);
    }

    public async Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken ct)
    {
        return await context.Certificates
            .AsNoTracking()
            .AnyAsync(c => c.StudentId == studentId && c.CourseId == courseId, ct);
    }

    public async Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Certificate> query = context.Certificates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search}%";
            query = query.Where(c => EF.Functions.ILike(c.SerialNumber, pattern));
        }

        var totalCount = await query.CountAsync(ct);
        query = request.OrderBy switch
        {
            "IssuedAt" => request.Descending ? query.OrderByDescending(c => c.IssuedAt) : query.OrderBy(c => c.IssuedAt),
            "CourseId" => request.Descending ? query.OrderByDescending(c => c.CourseId) : query.OrderBy(c => c.CourseId),
            _ => request.Descending ? query.OrderByDescending(c => c.SerialNumber) : query.OrderBy(c => c.SerialNumber)
        };

        var certificates = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.CourseId))
            .ToListAsync(ct);

        return new PagedResponse<CertificateResponseDto>
        {
            Items = certificates,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CertificateResponseDto?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out var certificateId))
        {
            logger.LogWarning("Invalid certificate ID format {CertificateId}", id);
            return null;
        }

        var certificate = await context.Certificates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == certificateId);

        if(certificate is null)
        {
            logger.LogWarning("Certificate {CertificateId} not found", id);
            return null;
        }

        return new CertificateResponseDto(
            certificate.Id,
            certificate.SerialNumber,
            certificate.IssuedAt,
            certificate.StudentId,
            certificate.CourseId);
    }

    public async Task<IReadOnlyList<CertificateResponseDto>> GetAllAsync()
    {
        var certificates = await context.Certificates
            .AsNoTracking()
            .ToListAsync();

        var records = certificates
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.CourseId))
            .ToList();

        logger.LogInformation("Retrieved {Count} certificates", records.Count);
        return records;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var certificateId))
        {
            logger.LogWarning("Invalid certificate ID format {CertificateId}", id);
            return false;
        }

        var certificate = await context.Certificates.FirstOrDefaultAsync(c => c.Id == certificateId);
        
        if(certificate is null)
        {
            logger.LogWarning("Delete failed certificate {CertificateId} not found", id);
            return false;
        }

        context.Certificates.Remove(certificate);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted certificate {CertificateId}", id);
        return true;
    }

    // Explicit interface implementation to ensure interface contract is satisfied.
    
}

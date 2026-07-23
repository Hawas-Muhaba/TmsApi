using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedEnrollmentService
{
    private readonly HybridCache _cache;
    private readonly IEnrollmentService _service;
    private readonly ILogger<CachedEnrollmentService> _logger;

    public CachedEnrollmentService(
        HybridCache cache,
        IEnrollmentService service,
        ILogger<CachedEnrollmentService> logger)
    {
        _cache = cache;
        _service = service;
        _logger = logger;
    }

    public async Task<EnrollmentResponseDto?> GetEnrollmentByIdAsync(int id, CancellationToken cancellationToken)
    {
        var key = CacheKeys.Enrollment(id);

        var dto = await _cache.GetOrCreateAsync<EnrollmentResponseDto?>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var enrollment = await _service.GetByIdAsync(id, token);
                
                if (enrollment is null)
                    _logger.LogInformation("Enrollment {Id} not found in database", id);
                
                return enrollment;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.EnrollmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return dto;
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllEnrollmentsAsync(CancellationToken cancellationToken)
    {
        var key = CacheKeys.EnrollmentsAll;

        var enrollments = await _cache.GetOrCreateAsync<IReadOnlyList<EnrollmentResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var enrolments = await _service.GetAllAsync();
                
                _logger.LogInformation("Retrieved {Count} enrollments from database for {Key}", enrolments.Count, key);
                return enrolments;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.EnrollmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return enrollments;
    }

    public async Task<PagedResponse<EnrollmentResponseDto>> GetEnrollmentsAsync(int courseId, PagedRequest request, CancellationToken cancellationToken)
    {
        var key = $"{CacheKeys.EnrollmentsAll}:course_{courseId}:page_{request.Page}:size_{request.PageSize}";

        var pagedResult = await _cache.GetOrCreateAsync<PagedResponse<EnrollmentResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var enrolls = await _service.GetEnrollmentsAsync(courseId, request, token);
                
                _logger.LogInformation("Retrieved {Count} enrollments from database for {Key}", enrolls.Items.Count, key);
                return enrolls;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.EnrollmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return pagedResult;
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken)
    {
        var key = $"{CacheKeys.EnrollmentsAll}:student_{studentId}";

        var enrollments = await _cache.GetOrCreateAsync<IReadOnlyList<EnrollmentResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var enrolls = await _service.GetByStudentIdAsync(studentId, token);
                
                _logger.LogInformation("Retrieved {Count} enrollments for student {StudentId} from database for {Key}", enrolls.Count, studentId, key);
                return enrolls;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.EnrollmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return enrollments;
    }

    public async Task InvalidateEnrollmentCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.EnrollmentsTag);
        await _cache.RemoveByTagAsync(CacheKeys.EnrollmentsTag, cancellationToken);
    }
}

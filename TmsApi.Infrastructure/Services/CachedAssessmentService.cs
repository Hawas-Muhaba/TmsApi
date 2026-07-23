using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedAssessmentService
{
    private readonly HybridCache _cache;
    private readonly IAssessmentService _service;
    private readonly ILogger<CachedAssessmentService> _logger;

    public CachedAssessmentService(
        HybridCache cache,
        IAssessmentService service,
        ILogger<CachedAssessmentService> logger)
    {
        _cache = cache;
        _service = service;
        _logger = logger;
    }

    public async Task<AssessmentResponseDto?> GetAssessmentByIdAsync(string id, CancellationToken cancellationToken)
    {
        var key = CacheKeys.Assessment(id);

        var dto = await _cache.GetOrCreateAsync<AssessmentResponseDto?>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var assessment = await _service.GetByIdAsync(id);
                
                if (assessment is null)
                    _logger.LogInformation("Assessment {Id} not found in database", id);
                
                return assessment;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.AssessmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return dto;
    }

    public async Task<IReadOnlyList<AssessmentResponseDto>> GetAllAssessmentsAsync(CancellationToken cancellationToken)
    {
        var key = CacheKeys.AssessmentsAll;

        var assessments = await _cache.GetOrCreateAsync<IReadOnlyList<AssessmentResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var assesses = await _service.GetAllAsync();
                
                _logger.LogInformation("Retrieved {Count} assessments from database for {Key}", assesses.Count, key);
                return assesses;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.AssessmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return assessments;
    }

    public async Task<PagedResponse<AssessmentResponseDto>> GetAssessmentsAsync(PagedRequest request, CancellationToken cancellationToken)
    {
        var key = $"{CacheKeys.AssessmentsAll}:page_{request.Page}:size_{request.PageSize}";

        var pagedResult = await _cache.GetOrCreateAsync<PagedResponse<AssessmentResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var assesses = await _service.GetAssessmentsAsync(request, token);
                
                _logger.LogInformation("Retrieved {Count} assessments from database for {Key}", assesses.Items.Count, key);
                return assesses;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.AssessmentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return pagedResult;
    }

    public async Task InvalidateAssessmentCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.AssessmentsTag);
        await _cache.RemoveByTagAsync(CacheKeys.AssessmentsTag, cancellationToken);
    }
}

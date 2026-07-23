using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService
{
    private readonly HybridCache _cache;
    private readonly ICourseService _service;
    private readonly ILogger<CachedCourseService> _logger;

    public CachedCourseService(
        HybridCache cache,
        ICourseService service,
        ILogger<CachedCourseService> logger)
    {
        _cache = cache;
        _service = service;
        _logger = logger;
    }

    public async Task<CourseResponseDto> GetCourseAsync(string code, CancellationToken cancellationToken)
    {
        var key = CacheKeys.Course(code);

        var dto = await _cache.GetOrCreateAsync<CourseResponseDto>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var course = await _service.GetByCodeAsync(code, token);
                
                if (course is null)
                    throw new KeyNotFoundException($"Course {code} not found.");
                
                _logger.LogInformation("Retrieved from database for {Key}", key);
                return course;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return dto;
    }

    public async Task<List<CourseResponseDto>> GetAllCoursesAsync(CancellationToken cancellationToken)
    {
        var key = CacheKeys.CoursesAll;

        var list = await _cache.GetOrCreateAsync<List<CourseResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var request = new PagedRequest { Page = 1, PageSize = 1000 };
                var pagedResult = await _service.GetCoursesAsync(request, token);
                
                _logger.LogInformation("Retrieved {Count} courses from database for {Key}", pagedResult.Items.Count, key);
                return pagedResult.Items.ToList();
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return list;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await _cache.RemoveByTagAsync(CacheKeys.CoursesTag, cancellationToken);
    }
}

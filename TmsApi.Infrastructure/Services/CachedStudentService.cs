using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedStudentService
{
    private readonly HybridCache _cache;
    private readonly IStudentService _service;
    private readonly ILogger<CachedStudentService> _logger;

    public CachedStudentService(
        HybridCache cache,
        IStudentService service,
        ILogger<CachedStudentService> logger)
    {
        _cache = cache;
        _service = service;
        _logger = logger;
    }

    public async Task<StudentResponseDto?> GetStudentByIdAsync(string id, CancellationToken cancellationToken)
    {
        var key = CacheKeys.Student(id);

        var dto = await _cache.GetOrCreateAsync<StudentResponseDto?>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var student = await _service.GetByIdAsync(id);
                
                if (student is null)
                    _logger.LogInformation("Student {Id} not found in database", id);
                
                return student;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.StudentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return dto;
    }

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken cancellationToken)
    {
        var key = $"{CacheKeys.StudentsAll}:page_{request.Page}:size_{request.PageSize}";

        var pagedResult = await _cache.GetOrCreateAsync<PagedResponse<StudentResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var students = await _service.GetStudentsAsync(request, token);
                
                _logger.LogInformation("Retrieved {Count} students from database for {Key}", students.Items.Count, key);
                return students;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.StudentsTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return pagedResult;
    }

    public async Task InvalidateStudentCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.StudentsTag);
        await _cache.RemoveByTagAsync(CacheKeys.StudentsTag, cancellationToken);
    }
}

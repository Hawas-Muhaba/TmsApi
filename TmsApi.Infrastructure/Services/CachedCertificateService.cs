using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCertificateService
{
    private readonly HybridCache _cache;
    private readonly ICertificateService _service;
    private readonly ILogger<CachedCertificateService> _logger;

    public CachedCertificateService(
        HybridCache cache,
        ICertificateService service,
        ILogger<CachedCertificateService> logger)
    {
        _cache = cache;
        _service = service;
        _logger = logger;
    }

    public async Task<CertificateResponseDto?> GetCertificateByIdAsync(string id, CancellationToken cancellationToken)
    {
        var key = CacheKeys.Certificate(id);

        var dto = await _cache.GetOrCreateAsync<CertificateResponseDto?>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var certificate = await _service.GetByIdAsync(id);
                
                if (certificate is null)
                    _logger.LogInformation("Certificate {Id} not found in database", id);
                
                return certificate;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.CertificatesTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return dto;
    }

    public async Task<IReadOnlyList<CertificateResponseDto>> GetAllCertificatesAsync(CancellationToken cancellationToken)
    {
        var key = CacheKeys.CertificatesAll;

        var certificates = await _cache.GetOrCreateAsync<IReadOnlyList<CertificateResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var certs = await _service.GetAllAsync();
                
                _logger.LogInformation("Retrieved {Count} certificates from database for {Key}", certs.Count, key);
                return certs;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.CertificatesTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return certificates;
    }

    public async Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(PagedRequest request, CancellationToken cancellationToken)
    {
        var key = $"{CacheKeys.CertificatesAll}:page_{request.Page}:size_{request.PageSize}";

        var pagedResult = await _cache.GetOrCreateAsync<PagedResponse<CertificateResponseDto>>(
            key,
            async (token) =>
            {
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                var certs = await _service.GetCertificatesAsync(request, token);
                
                _logger.LogInformation("Retrieved {Count} certificates from database for {Key}", certs.Items.Count, key);
                return certs;
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: [CacheKeys.CertificatesTag],
            cancellationToken: cancellationToken);

        _logger.LogInformation("Cache HIT for {Key}", key);
        return pagedResult;
    }

    public async Task InvalidateCertificateCacheAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CertificatesTag);
        await _cache.RemoveByTagAsync(CacheKeys.CertificatesTag, cancellationToken);
    }
}

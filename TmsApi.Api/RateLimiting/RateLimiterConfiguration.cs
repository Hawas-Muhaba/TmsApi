using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.RateLimiting;

public static class RateLimiterConfiguration
{
    public static void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = OnRejectedAsync;
        
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(CreateLimiter);
        
        // Named policy for transcript endpoint
        options.AddConcurrencyLimiter("transcripts", opt =>
        {
            opt.PermitLimit = 5;            // 5 in-flight transcript requests maximum
            opt.QueueLimit = 20;            // queue up to 20 requests
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });

        // Named policy for heavy search requests
        options.AddTokenBucketLimiter("search", opt =>
        {
            opt.TokenLimit = 10;
            opt.TokensPerPeriod = 5;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 2;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.AutoReplenishment = true;
        });
    }

    private static RateLimitPartition<string> CreateLimiter(HttpContext httpContext)
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        var (tokensPerPeriod, period) = tier switch
        {
            ApiKeyTier.Free => (10, TimeSpan.FromSeconds(60)),      // 10 req/min
            ApiKeyTier.Paid => (100, TimeSpan.FromSeconds(60)),     // 100 req/min
            _ => (5, TimeSpan.FromSeconds(60))                      // 5 req/min (anonymous)
        };

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = tokensPerPeriod,
                Window = period,
                SegmentsPerWindow = 2,
                AutoReplenishment = true
            });
    }

    private static RateLimitPartition<string> CreateTranscriptPolicy(HttpContext httpContext)
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        // Transcript endpoint is more resource-intensive, so lower limits
        var (tokensPerPeriod, period) = tier switch
        {
            ApiKeyTier.Free => (3, TimeSpan.FromSeconds(60)),       // 3 req/min
            ApiKeyTier.Paid => (20, TimeSpan.FromSeconds(60)),      // 20 req/min
            _ => (1, TimeSpan.FromSeconds(60))                      // 1 req/min (anonymous)
        };

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = tokensPerPeriod,
                Window = period,
                SegmentsPerWindow = 2,
                AutoReplenishment = true
            });
    }

    private static ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken ct)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata("RETRY_AFTER", out var retryAfter))
        {
            if (retryAfter is TimeSpan ts)
                context.HttpContext.Response.Headers.RetryAfter = ts.TotalSeconds.ToString("F0");
        }

        return ValueTask.CompletedTask;
    }
}

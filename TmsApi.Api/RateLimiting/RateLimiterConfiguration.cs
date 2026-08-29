using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
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

        options.AddTokenBucketLimiter("students", opt =>
        {
            opt.TokenLimit = 15;
            opt.TokensPerPeriod = 7;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 5;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.AutoReplenishment = true;
        });

        options.AddTokenBucketLimiter("certificates", opt =>
        {
            opt.TokenLimit = 15;
            opt.TokensPerPeriod = 7;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 5;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.AutoReplenishment = true;
        });

        options.AddTokenBucketLimiter("assessments", opt =>
        {
            opt.TokenLimit = 15;
            opt.TokensPerPeriod = 7;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 5;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.AutoReplenishment = true;
        });

        options.AddTokenBucketLimiter("enrollments", opt =>
        {
            opt.TokenLimit = 10;
            opt.TokensPerPeriod = 5;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 3;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.AutoReplenishment = true;
        });
    }

    private static RateLimitPartition<string> CreateLimiter(HttpContext httpContext)
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                $"paid:{partitionKey}", _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 200,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),
            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                $"free:{partitionKey}", _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 30,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),
            _ => RateLimitPartition.GetTokenBucketLimiter(
                $"anon:{partitionKey}", _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                })
        };
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

        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata)
            && retryAfterMetadata is TimeSpan retryAfterSpan)
        {
            retryAfter = Math.Max(1, (int)Math.Ceiling(retryAfterSpan.TotalSeconds)).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        return WriteRejectedResponseAsync(context.HttpContext, retryAfter, ct);
    }

    private static async ValueTask WriteRejectedResponseAsync(
        HttpContext httpContext, string retryAfter, CancellationToken ct)
    {
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = $"Too many requests. Retry after {retryAfter} seconds.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tms.local/errors/rate_limit_exceeded"
        }, options: null, contentType: "application/problem+json", cancellationToken: ct);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Persistence;

public class AssessmentService(TmsDbContext context, ILogger<AssessmentService> logger): IAssessmentService
{
    public async Task<AssessmentResponseDto> CreateAsync(CreateAssessmentRequest request, CancellationToken ct)
    {
        var assessment = new Assessment
        {
            Title = request.Title,
            MaxScore = request.MaxScore,
            Weight = request.Weight,
            CourseId = request.CourseId
        };

        context.Assessments.Add(assessment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created assessment {Title} {MaxScore} weight {Weight} for course {CourseId}",
            request.Title, request.MaxScore, request.Weight, request.CourseId);

        return new AssessmentResponseDto(assessment.Id, assessment.Title, assessment.MaxScore, assessment.Weight, assessment.CourseId);
    }

    public async Task<bool> ExistsAsync(string title, int courseId, CancellationToken ct)
    {
        return await context.Assessments
            .AsNoTracking()
            .AnyAsync(a => a.Title == title && a.CourseId == courseId, ct);
    }

    public async Task<PagedResponse<AssessmentResponseDto>> GetAssessmentsAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Assessment> query = context.Assessments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search}%";
            query = query.Where(a => EF.Functions.ILike(a.Title, pattern));
        }

        var totalCount = await query.CountAsync(ct);
        query = request.OrderBy switch
        {
            "MaxScore" => request.Descending ? query.OrderByDescending(a => a.MaxScore) : query.OrderBy(a => a.MaxScore),
            "Weight" => request.Descending ? query.OrderByDescending(a => a.Weight) : query.OrderBy(a => a.Weight),
            _ => request.Descending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title)
        };

        var assessments = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .ToListAsync(ct);

        return new PagedResponse<AssessmentResponseDto>
        {
            Items = assessments,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AssessmentResponseDto?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out var assessmentId))
        {
            logger.LogWarning("Invalid assessment ID format {AssessmentId}", id);
            return null;
        }

        var assessment = await context.Assessments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if(assessment is null)
        {
            logger.LogWarning("Assessment {AssessmentId} not found", id);
            return null;
        }

        return new AssessmentResponseDto(
            assessment.Id,
            assessment.Title,
            assessment.MaxScore,
            assessment.Weight,
            assessment.CourseId);
    }

    public async Task<IReadOnlyList<AssessmentResponseDto>> GetAllAsync()
    {
        var assessments = await context.Assessments
            .AsNoTracking()
            .ToListAsync();

        var records = assessments
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .ToList();

        logger.LogInformation("Retrieved {Count} assessments", records.Count);
        return records;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var assessmentId))
        {
            logger.LogWarning("Invalid assessment ID format {AssessmentId}", id);
            return false;
        }

        var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
        
        if(assessment is null)
        {
            logger.LogWarning("Delete failed assessment {AssessmentId} not found", id);
            return false;
        }

        context.Assessments.Remove(assessment);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted assessment {AssessmentId}", id);
        return true;
    }
}
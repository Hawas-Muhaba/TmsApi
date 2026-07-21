using MediatR;
using TmsApi.Application.Assessments.Commands;
using TmsApi.Application.Assessments.Queries;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Assessments.Handlers;

public class CreateAssessmentHandler(IAssessmentService assessmentService)
    : IRequestHandler<CreateAssessmentCommand, Result<AssessmentResponseDto, AssessmentError>>
{
    public async Task<Result<AssessmentResponseDto, AssessmentError>> Handle(
        CreateAssessmentCommand request, CancellationToken ct)
    {
        var exists = await assessmentService.ExistsAsync(request.Title, request.CourseId, ct);
        if (exists)
            return Result<AssessmentResponseDto, AssessmentError>.Failure(
                AssessmentError.AlreadyExists(request.Title, request.CourseId));

        var result = await assessmentService.CreateAsync(
            new CreateAssessmentRequest(request.Title, request.MaxScore, request.Weight, request.CourseId), ct);
        return result is not null
            ? Result<AssessmentResponseDto, AssessmentError>.Success(result)
            : Result<AssessmentResponseDto, AssessmentError>.Failure(AssessmentError.InvalidData);
    }
}

public class DeleteAssessmentHandler(IAssessmentService assessmentService)
    : IRequestHandler<DeleteAssessmentCommand, Result<Unit, AssessmentError>>
{
    public async Task<Result<Unit, AssessmentError>> Handle(
        DeleteAssessmentCommand request, CancellationToken ct)
    {
        var deleted = await assessmentService.DeleteAsync(request.Id);
        return deleted
            ? Result<Unit, AssessmentError>.Success(Unit.Value)
            : Result<Unit, AssessmentError>.Failure(AssessmentError.AssessmentNotFound(request.Id));
    }
}

public class GetAssessmentHandler(IAssessmentService assessmentService)
    : IRequestHandler<GetAssessmentQuery, Result<AssessmentResponseDto, AssessmentError>>
{
    public async Task<Result<AssessmentResponseDto, AssessmentError>> Handle(
        GetAssessmentQuery request, CancellationToken ct)
    {
        var assessment = await assessmentService.GetByIdAsync(request.Id);
        return assessment is not null
            ? Result<AssessmentResponseDto, AssessmentError>.Success(assessment)
            : Result<AssessmentResponseDto, AssessmentError>.Failure(AssessmentError.AssessmentNotFound(request.Id));
    }
}

public class GetAssessmentsHandler(IAssessmentService assessmentService)
    : IRequestHandler<GetAssessmentsQuery, PagedResponse<AssessmentResponseDto>>
{
    public async Task<PagedResponse<AssessmentResponseDto>> Handle(
        GetAssessmentsQuery request, CancellationToken ct)
    {
        return await assessmentService.GetAssessmentsAsync(request.Request, ct);
    }
}

using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Assessments.Queries;

public record GetAssessmentQuery(string Id)
    : IRequest<Result<AssessmentResponseDto, AssessmentError>>;

public record GetAssessmentsQuery(PagedRequest Request)
    : IRequest<PagedResponse<AssessmentResponseDto>>;

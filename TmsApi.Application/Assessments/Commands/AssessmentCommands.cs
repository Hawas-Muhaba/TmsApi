using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Assessments.Commands;

public record CreateAssessmentCommand(string Title, decimal MaxScore, decimal Weight, int CourseId)
    : IRequest<Result<AssessmentResponseDto, AssessmentError>>;

public record DeleteAssessmentCommand(string Id)
    : IRequest<Result<Unit, AssessmentError>>;

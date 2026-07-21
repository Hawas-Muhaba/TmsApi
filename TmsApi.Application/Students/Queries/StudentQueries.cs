using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Students.Queries;

public record GetStudentQuery(string Id)
    : IRequest<Result<StudentResponseDto, StudentError>>;

public record GetStudentsQuery(PagedRequest Request)
    : IRequest<PagedResponse<StudentResponseDto>>;

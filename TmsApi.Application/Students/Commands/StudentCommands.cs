using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Students.Commands;

public record CreateStudentCommand(string Name)
    : IRequest<Result<StudentResponseDto, StudentError>>;

public record DeleteStudentCommand(string Id)
    : IRequest<Result<Unit, StudentError>>;

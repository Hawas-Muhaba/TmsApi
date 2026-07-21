using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Students.Commands;
using TmsApi.Application.Students.Queries;

namespace TmsApi.Application.Students.Handlers;

public class CreateStudentHandler(IStudentService studentService)
    : IRequestHandler<CreateStudentCommand, Result<StudentResponseDto, StudentError>>
{
    public async Task<Result<StudentResponseDto, StudentError>> Handle(
        CreateStudentCommand request, CancellationToken ct)
    {
        var exists = await studentService.ExistsAsync(request.Name, ct);
        if (exists)
            return Result<StudentResponseDto, StudentError>.Failure(
                StudentError.NameAlreadyExists(request.Name));

        var result = await studentService.CreateAsync(
            new CreateStudentRequest(request.Name, ""), ct);
        return result is not null
            ? Result<StudentResponseDto, StudentError>.Success(result)
            : Result<StudentResponseDto, StudentError>.Failure(StudentError.InvalidData);
    }
}

public class DeleteStudentHandler(IStudentService studentService)
    : IRequestHandler<DeleteStudentCommand, Result<Unit, StudentError>>
{
    public async Task<Result<Unit, StudentError>> Handle(
        DeleteStudentCommand request, CancellationToken ct)
    {
        var deleted = await studentService.DeleteAsync(request.Id);
        return deleted
            ? Result<Unit, StudentError>.Success(Unit.Value)
            : Result<Unit, StudentError>.Failure(StudentError.StudentNotFound(request.Id));
    }
}

public class GetStudentHandler(IStudentService studentService)
    : IRequestHandler<GetStudentQuery, Result<StudentResponseDto, StudentError>>
{
    public async Task<Result<StudentResponseDto, StudentError>> Handle(
        GetStudentQuery request, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(request.Id);
        return student is not null
            ? Result<StudentResponseDto, StudentError>.Success(student)
            : Result<StudentResponseDto, StudentError>.Failure(StudentError.StudentNotFound(request.Id));
    }
}

public class GetStudentsHandler(IStudentService studentService)
    : IRequestHandler<GetStudentsQuery, PagedResponse<StudentResponseDto>>
{
    public async Task<PagedResponse<StudentResponseDto>> Handle(
        GetStudentsQuery request, CancellationToken ct)
    {
        return await studentService.GetStudentsAsync(request.Request, ct);
    }
}

using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Handlers;

public class CreateCourseHandler(ICourseService courseService)
    : IRequestHandler<CreateCourseCommand, Result<CourseResponseDto, CourseError>>
{
    public async Task<Result<CourseResponseDto, CourseError>> Handle(
        CreateCourseCommand request, CancellationToken ct)
    {
        var exists = await courseService.CodeExistsAsync(request.Code, ct);
        if (exists)
            return Result<CourseResponseDto, CourseError>.Failure(
                CourseError.CodeAlreadyExists(request.Code));

        var result = await courseService.CreateAsync(
            new CreateCourseRequest(request.Code, request.Title, request.MaxCapacity), ct);
        return result is not null
            ? Result<CourseResponseDto, CourseError>.Success(result)
            : Result<CourseResponseDto, CourseError>.Failure(CourseError.CodeAlreadyExists(request.Code));
    }
}

public class DeleteCourseHandler(ICourseService courseService)
    : IRequestHandler<DeleteCourseCommand, Result<Unit, CourseError>>
{
    public async Task<Result<Unit, CourseError>> Handle(
        DeleteCourseCommand request, CancellationToken ct)
    {
        var deleted = await courseService.DeleteAsync(request.Id.ToString());
        return deleted
            ? Result<Unit, CourseError>.Success(Unit.Value)
            : Result<Unit, CourseError>.Failure(CourseError.CourseNotFound(request.Id));
    }
}

public class GetCourseHandler(ICourseService courseService)
    : IRequestHandler<GetCourseQuery, Result<CourseResponseDto, CourseError>>
{
    public async Task<Result<CourseResponseDto, CourseError>> Handle(
        GetCourseQuery request, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(request.Id, ct);
        return course is not null
            ? Result<CourseResponseDto, CourseError>.Success(course)
            : Result<CourseResponseDto, CourseError>.Failure(CourseError.CourseNotFound(request.Id));
    }
}

public class GetCoursesHandler(ICourseService courseService)
    : IRequestHandler<GetCoursesQuery, PagedResponse<CourseResponseDto>>
{
    public async Task<PagedResponse<CourseResponseDto>> Handle(
        GetCoursesQuery request, CancellationToken ct)
    {
        return await courseService.GetCoursesAsync(request.Request, ct);
    }
}

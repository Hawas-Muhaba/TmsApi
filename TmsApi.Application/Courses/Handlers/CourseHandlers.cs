using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Handlers;

public class CreateCourseHandler(ICourseService courseService, ICachedCourseService cachedCourseService)
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
        if (result is not null)
        {
            await cachedCourseService.InvalidateCourseCacheAsync(ct);
            return Result<CourseResponseDto, CourseError>.Success(result);
        }

        return Result<CourseResponseDto, CourseError>.Failure(
            CourseError.CodeAlreadyExists(request.Code));
    }
}

public class DeleteCourseHandler(ICourseService courseService, ICachedCourseService cachedCourseService)
    : IRequestHandler<DeleteCourseCommand, Result<Unit, CourseError>>
{
    public async Task<Result<Unit, CourseError>> Handle(
        DeleteCourseCommand request, CancellationToken ct)
    {
        var deleted = await courseService.DeleteAsync(request.Id.ToString());
        if (deleted)
        {
            await cachedCourseService.InvalidateCourseCacheAsync(ct);
            return Result<Unit, CourseError>.Success(Unit.Value);
        }

        return Result<Unit, CourseError>.Failure(
            CourseError.CourseNotFound(request.Id));
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

public class GetCoursesHandler(ICachedCourseService cachedCourseService)
    : IRequestHandler<GetCoursesQuery, PagedResponse<CourseResponseDto>>
{
    public async Task<PagedResponse<CourseResponseDto>> Handle(
        GetCoursesQuery request, CancellationToken ct)
    {
        var allCourses = await cachedCourseService.GetAllCoursesAsync(ct);
        var filtered = allCourses.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            filtered = filtered.Where(course =>
                course.Title.Contains(request.Request.Search, StringComparison.OrdinalIgnoreCase));
        }

        filtered = request.Request.OrderBy switch
        {
            "Code" => request.Request.Descending
                ? filtered.OrderByDescending(course => course.Code)
                : filtered.OrderBy(course => course.Code),
            "MaxCapacity" => request.Request.Descending
                ? filtered.OrderByDescending(course => course.MaxCapacity)
                : filtered.OrderBy(course => course.MaxCapacity),
            _ => request.Request.Descending
                ? filtered.OrderByDescending(course => course.Title)
                : filtered.OrderBy(course => course.Title)
        };

        var totalCount = filtered.Count();
        var items = filtered
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToList();

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

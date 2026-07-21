using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Queries;

public record GetCourseQuery(int Id)
    : IRequest<Result<CourseResponseDto, CourseError>>;

public record GetCoursesQuery(PagedRequest Request)
    : IRequest<PagedResponse<CourseResponseDto>>;

using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Commands;

public record CreateCourseCommand(string Code, string Title, int MaxCapacity)
    : IRequest<Result<CourseResponseDto, CourseError>>;

public record DeleteCourseCommand(int Id)
    : IRequest<Result<Unit, CourseError>>;

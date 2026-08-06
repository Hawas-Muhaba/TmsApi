using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Common;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.DTOs;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[ApiVersion("2.0")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(IMediator mediator, LinkGenerator linkGenerator) : ControllerBase
{
    // GET /api/courses
    // [HttpGet]
    // public async Task<IActionResult> GetAll()
    //     => Ok(await courseService.GetAllAsync());

    [HttpGet]
    [EnableRateLimiting("search")]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var query = new GetCoursesQuery(request);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var query = new GetCourseQuery(id);
        var result = await mediator.Send(query, ct);
        
        return result.Match<IActionResult>(
            onSuccess: course =>
            {
                var selfHref = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id = course.Id }) ?? string.Empty;
                var enrollmentsPath = linkGenerator.GetPathByAction(
                    HttpContext,
                    action: "GetEnrollments",
                    controller: "Enrollments",
                    values: new { courseId = course.Id }) ?? string.Empty;

                var links = new List<LinkDto>
                {
                    new LinkDto(selfHref, "self", "GET"),
                    new LinkDto(selfHref, "update", "PUT"),
                    new LinkDto(selfHref, "delete", "DELETE"),
                    new LinkDto(enrollmentsPath, "enrollments", "GET")
                };

                if (course.EnrollmentCount < course.MaxCapacity)
                {
                    links.Add(new LinkDto(enrollmentsPath, "enroll", "POST"));
                }

                var detailDto = new CourseDetailDto
                {
                    Id = course.Id,
                    Code = course.Code,
                    Title = course.Title,
                    MaxCapacity = course.MaxCapacity,
                    EnrollmentCount = course.EnrollmentCount,
                    Links = links
                };

                return Ok(detailDto);
            },
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status400BadRequest
                };
                return Problem(statusCode: status, title: "Course operation failed",
                    detail: error.Message, type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        var command = new CreateCourseCommand(request.Code, request.Title, request.MaxCapacity);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(nameof(GetCourseById), new { id = created.Id }, created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "code_exists" => StatusCodes.Status409Conflict,
                    "invalid_code" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };
                return Problem(statusCode: status, title: "Course creation failed",
                    detail: error.Message, type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Deletes a course by ID. Returns 204 if successful, 404 if not found.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var command = new DeleteCourseCommand(id);
        var result = await mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            onSuccess: _ => NoContent(),
            onFailure: error => Problem(statusCode: StatusCodes.Status404NotFound, title: "Course not found",
                detail: error.Message, type: $"https://tms.local/errors/{error.Code}"));
    }
}




// git commit -m  "
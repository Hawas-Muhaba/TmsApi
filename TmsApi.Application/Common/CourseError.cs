namespace TmsApi.Application.Common;

public sealed record CourseError(string Code, string Message)
{
    public static CourseError CourseNotFound(int id) =>
        new("course_not_found", $"Course with ID {id} was not found.");

    public static CourseError CodeAlreadyExists(string code) =>
        new("code_exists", $"A course with code '{code}' already exists.");

    public static CourseError InvalidCode(string code) =>
        new("invalid_code", $"Course code '{code}' is invalid.");
}

namespace TmsApi.Application.Common;

public sealed record StudentError(string Code, string Message)
{
    public static StudentError StudentNotFound(string id) =>
        new("student_not_found", $"Student with ID '{id}' was not found.");

    public static StudentError NameAlreadyExists(string name) =>
        new("name_exists", $"A student named '{name}' already exists.");

    public static StudentError InvalidData =>
        new("invalid_data", "Student data is invalid.");
}

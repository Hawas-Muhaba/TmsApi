namespace TmsApi.Application.Common;

public sealed record AssessmentError(string Code, string Message)
{
    public static AssessmentError AssessmentNotFound(string id) =>
        new("assessment_not_found", $"Assessment with ID '{id}' was not found.");

    public static AssessmentError AlreadyExists(string title, int courseId) =>
        new("assessment_exists", $"An assessment with title '{title}' already exists for course {courseId}.");

    public static AssessmentError InvalidData =>
        new("invalid_data", "Assessment data is invalid.");
}

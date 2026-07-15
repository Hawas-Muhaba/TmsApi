namespace TmsApi.Dtos;

public record EnrollmentResponseDto(int Id, int CourseId, string CourseCode, string CourseTitle, int StudentId, DateTime EnrollmentAt);
using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateEnrollmentRequest(
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer")]
    int CourseId,
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer")]
    int StudentId
);

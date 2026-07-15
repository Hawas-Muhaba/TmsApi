using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateAssessmentRequest(
    [Required, MaxLength(200)]
    string Title,
    [Range(0.1, 1000, ErrorMessage = "MaxScore must be a positive value")]
    decimal MaxScore,
    [Range(0.0, 1.0, ErrorMessage = "Weight must be between 0 and 1")]
    decimal Weight,
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer")]
    int CourseId
);

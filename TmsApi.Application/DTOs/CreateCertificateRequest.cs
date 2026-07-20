using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record CreateCertificateRequest(
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer")]
    int StudentId,
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer")]
    int CourseId
);

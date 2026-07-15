using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateStudentRequest(
    [Required, MaxLength(150)]
    string Name,
    [Required, EmailAddress]
    string Email
);

using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

    public record CreateCourseRequest(
        [Required, RegularExpression(@"^[A-Z]{2,4}-\d{3}$", ErrorMessage = "Code must be in the format 'XXX-000' (e.g., CSE-101)")]
        string Code,
        [Required, MaxLength(200)]
        string Title,
        [Range(1, 200)]
        int MaxCapacity
    );

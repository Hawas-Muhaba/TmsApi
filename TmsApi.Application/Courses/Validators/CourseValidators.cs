using FluentValidation;
using TmsApi.Application.Courses.Commands;

namespace TmsApi.Application.Courses.Validators;

public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(cmd => cmd.Code)
            .NotEmpty().WithMessage("Course code is required.")
            .Matches(@"^[A-Z]{3}-\d{3}$").WithMessage("Course code must follow the format XXX-000 (3 uppercase letters, hyphen, 3 digits).");

        RuleFor(cmd => cmd.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters.");

        RuleFor(cmd => cmd.MaxCapacity)
            .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Maximum capacity cannot exceed 1000.");
    }
}

public class DeleteCourseValidator : AbstractValidator<DeleteCourseCommand>
{
    public DeleteCourseValidator()
    {
        RuleFor(cmd => cmd.Id)
            .GreaterThan(0).WithMessage("Course ID must be a positive integer.");
    }
}

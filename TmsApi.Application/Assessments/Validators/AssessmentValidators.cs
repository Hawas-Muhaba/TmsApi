using FluentValidation;
using TmsApi.Application.Assessments.Commands;

namespace TmsApi.Application.Assessments.Validators;

public class CreateAssessmentValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentValidator()
    {
        RuleFor(cmd => cmd.Title)
            .NotEmpty().WithMessage("Assessment title is required.")
            .MaximumLength(200).WithMessage("Assessment title cannot exceed 200 characters.");

        RuleFor(cmd => cmd.MaxScore)
            .GreaterThan(0).WithMessage("Maximum score must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Maximum score cannot exceed 1000.");

        RuleFor(cmd => cmd.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be 0 or greater.")
            .LessThanOrEqualTo(1).WithMessage("Weight cannot exceed 1.0 (100%).");

        RuleFor(cmd => cmd.CourseId)
            .GreaterThan(0).WithMessage("Course ID must be a positive integer.");
    }
}

public class DeleteAssessmentValidator : AbstractValidator<DeleteAssessmentCommand>
{
    public DeleteAssessmentValidator()
    {
        RuleFor(cmd => cmd.Id)
            .NotEmpty().WithMessage("Assessment ID is required.");
    }
}

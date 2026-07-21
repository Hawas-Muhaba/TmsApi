using FluentValidation;
using TmsApi.Application.Students.Commands;

namespace TmsApi.Application.Students.Validators;

public class CreateStudentValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentValidator()
    {
        RuleFor(cmd => cmd.Name)
            .NotEmpty().WithMessage("Student name is required.")
            .MinimumLength(2).WithMessage("Student name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Student name cannot exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Student name can only contain letters, spaces, hyphens, and apostrophes.");
    }
}

public class DeleteStudentValidator : AbstractValidator<DeleteStudentCommand>
{
    public DeleteStudentValidator()
    {
        RuleFor(cmd => cmd.Id)
            .NotEmpty().WithMessage("Student ID is required.");
    }
}

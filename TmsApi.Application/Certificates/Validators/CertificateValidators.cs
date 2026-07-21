using FluentValidation;
using TmsApi.Application.Certificates.Commands;

namespace TmsApi.Application.Certificates.Validators;

public class CreateCertificateValidator : AbstractValidator<CreateCertificateCommand>
{
    public CreateCertificateValidator()
    {
        RuleFor(cmd => cmd.StudentId)
            .GreaterThan(0).WithMessage("Student ID must be a positive integer.");

        RuleFor(cmd => cmd.CourseId)
            .GreaterThan(0).WithMessage("Course ID must be a positive integer.");
    }
}

public class DeleteCertificateValidator : AbstractValidator<DeleteCertificateCommand>
{
    public DeleteCertificateValidator()
    {
        RuleFor(cmd => cmd.Id)
            .NotEmpty().WithMessage("Certificate ID is required.");
    }
}

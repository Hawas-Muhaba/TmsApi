using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Certificates.Commands;

public record CreateCertificateCommand(int StudentId, int CourseId)
    : IRequest<Result<CertificateResponseDto, CertificateError>>;

public record DeleteCertificateCommand(string Id)
    : IRequest<Result<Unit, CertificateError>>;

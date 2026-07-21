using MediatR;
using TmsApi.Application.Certificates.Commands;
using TmsApi.Application.Certificates.Queries;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Certificates.Handlers;

public class CreateCertificateHandler(ICertificateService certificateService)
    : IRequestHandler<CreateCertificateCommand, Result<CertificateResponseDto, CertificateError>>
{
    public async Task<Result<CertificateResponseDto, CertificateError>> Handle(
        CreateCertificateCommand request, CancellationToken ct)
    {
        var exists = await certificateService.ExistsAsync(request.StudentId, request.CourseId, ct);
        if (exists)
            return Result<CertificateResponseDto, CertificateError>.Failure(
                CertificateError.AlreadyExists(request.StudentId, request.CourseId));

        var result = await certificateService.CreateAsync(
            new CreateCertificateRequest(request.StudentId, request.CourseId), ct);
        return result is not null
            ? Result<CertificateResponseDto, CertificateError>.Success(result)
            : Result<CertificateResponseDto, CertificateError>.Failure(
                CertificateError.AlreadyExists(request.StudentId, request.CourseId));
    }
}

public class DeleteCertificateHandler(ICertificateService certificateService)
    : IRequestHandler<DeleteCertificateCommand, Result<Unit, CertificateError>>
{
    public async Task<Result<Unit, CertificateError>> Handle(
        DeleteCertificateCommand request, CancellationToken ct)
    {
        var deleted = await certificateService.DeleteAsync(request.Id);
        return deleted
            ? Result<Unit, CertificateError>.Success(Unit.Value)
            : Result<Unit, CertificateError>.Failure(CertificateError.CertificateNotFound(request.Id));
    }
}

public class GetCertificateHandler(ICertificateService certificateService)
    : IRequestHandler<GetCertificateQuery, Result<CertificateResponseDto, CertificateError>>
{
    public async Task<Result<CertificateResponseDto, CertificateError>> Handle(
        GetCertificateQuery request, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(request.Id);
        return certificate is not null
            ? Result<CertificateResponseDto, CertificateError>.Success(certificate)
            : Result<CertificateResponseDto, CertificateError>.Failure(CertificateError.CertificateNotFound(request.Id));
    }
}

public class GetCertificatesHandler(ICertificateService certificateService)
    : IRequestHandler<GetCertificatesQuery, PagedResponse<CertificateResponseDto>>
{
    public async Task<PagedResponse<CertificateResponseDto>> Handle(
        GetCertificatesQuery request, CancellationToken ct)
    {
        return await certificateService.GetCertificatesAsync(request.Request, ct);
    }
}

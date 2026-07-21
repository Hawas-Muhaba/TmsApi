using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Certificates.Queries;

public record GetCertificateQuery(string Id)
    : IRequest<Result<CertificateResponseDto, CertificateError>>;

public record GetCertificatesQuery(PagedRequest Request)
    : IRequest<PagedResponse<CertificateResponseDto>>;

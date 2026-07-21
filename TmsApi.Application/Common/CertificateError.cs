namespace TmsApi.Application.Common;

public sealed record CertificateError(string Code, string Message)
{
    public static CertificateError CertificateNotFound(string id) =>
        new("certificate_not_found", $"Certificate with ID '{id}' was not found.");

    public static CertificateError AlreadyExists(int studentId, int courseId) =>
        new("certificate_exists", $"Student {studentId} already has a certificate for course {courseId}.");

    public static CertificateError InvalidData =>
        new("invalid_data", "Certificate data is invalid.");
}

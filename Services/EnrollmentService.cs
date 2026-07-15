using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<EnrollmentResponseDto> CreateAsync(CreateEnrollmentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = request.CourseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrolled student {StudentId} in course {CourseId} (enrollment {EnrollmentId})",
            request.StudentId, request.CourseId, enrollment.Id);

        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, ct);

        return new EnrollmentResponseDto(
            enrollment.Id,
            request.CourseId,
            course?.Code ?? "",
            course?.Title ?? "",
            request.StudentId,
            enrollment.EnrolledAt);
    }

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrolled student {StudentId} in course {CourseId} (enrollment {EnrollmentId})",
            request.StudentId, courseId, enrollment.Id);

        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);

        return new EnrollmentResponseDto(
            enrollment.Id,
            courseId,
            course?.Code ?? "",
            course?.Title ?? "",
            request.StudentId,
            enrollment.EnrolledAt);
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(int id, CancellationToken ct) 
    {
        var enrollment = await context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment is null)
        {
            return null;
        }

        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == enrollment.CourseId, ct);

        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            course?.Code ?? "",
            course?.Title ?? "",
            enrollment.StudentId,
            enrollment.EnrolledAt);
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) 
    {
        var enrollment = await context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.CourseId == courseId, ct);

        if (enrollment is null)
        {
            return null;
        }

        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);

        return new EnrollmentResponseDto(
            enrollment.Id,
            courseId,
            course?.Code ?? "",
            course?.Title ?? "",
            enrollment.StudentId,
            enrollment.EnrolledAt);
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync()
    {
        var enrollments = await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .ToListAsync();

        var records = enrollments
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course?.Code ?? "",
                e.Course?.Title ?? "",
                e.StudentId,
                e.EnrolledAt))
            .ToList();

        logger.LogInformation("Retrieved {Count} enrollments", records.Count);
        return records;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var enrollmentId))
        {
            logger.LogWarning("Invalid enrollment ID format {EnrollmentId}", id);
            return false;
        }

        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId);
        
        if (enrollment is null)
        {
            logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
            return false;
        }

        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        return true;
    }
}
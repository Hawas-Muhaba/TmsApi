using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    Task<int> ArchiveOldEnrollmentsAsync(DateTime cutoff);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id.ToString() == studentId || s.RegistrationNumber == studentId);

        if (student is null)
        {
            throw new InvalidOperationException($"Student '{studentId}' was not found.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        if (course is null)
        {
            throw new InvalidOperationException($"Course '{courseCode}' was not found.");
        }

        var existing = await _context.Enrollments
            .AnyAsync(e => e.StudentId == student.Id && e.CourseId == course.Id);

        if (existing)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode}",
                studentId, courseCode);

            var existingEnrollment = await _context.Enrollments
                .AsNoTracking()
                .FirstAsync(e => e.StudentId == student.Id && e.CourseId == course.Id);

            return ToRecord(existingEnrollment, course.Code);
        }

        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow,
            IsArchived = false
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId, courseCode, enrollment.Id);

        return ToRecord(enrollment, course.Code);
    }

    public async Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out var enrollmentId))
        {
            _logger.LogWarning("Enrollment {EnrollmentId} is not a valid numeric id", id);
            return null;
        }

        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
            return null;
        }

        return ToRecord(enrollment, enrollment.Course?.Code ?? string.Empty);
    }

    public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .OrderBy(e => e.EnrolledAt)
            .ToListAsync();

        return enrollments.Select(e => ToRecord(e, e.Course?.Code ?? string.Empty)).ToList();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var enrollmentId))
        {
            _logger.LogWarning("Delete failed: enrollment {EnrollmentId} is not a valid numeric id", id);
            return false;
        }

        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment is null)
        {
            _logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
            return false;
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        return true;
    }

    public async Task<int> ArchiveOldEnrollmentsAsync(DateTime cutoff)
    {
        var archivedCount = await _context.Enrollments
            .Where(e => !e.IsArchived && e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsArchived, true));

        _logger.LogInformation("Archived {ArchivedCount} enrollments older than {Cutoff}", archivedCount, cutoff);
        return archivedCount;
    }

    private static EnrollmentRecord ToRecord(Enrollment enrollment, string courseCode)
        => new(enrollment.Id.ToString(), enrollment.StudentId.ToString(), courseCode, enrollment.EnrolledAt);
}

public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);
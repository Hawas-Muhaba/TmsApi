using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context, ILogger<StudentService> logger) : IStudentService
{
    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var existing = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == request.Name && s.IsActive, ct);

        if (existing is not null)
        {
            logger.LogWarning("Duplicate student registration attempt {FullName} {Email} (record {StudentId})",
                request.Name, request.Email, existing.Id);
            return new StudentResponseDto(existing.Id, existing.RegistrationNumber, existing.Name, existing.GPA, existing.IsActive, existing.LastUpdated);
        }

        var student = new Student
        {
            RegistrationNumber = $"STU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6]}",
            Name = request.Name,
            GPA = 0m,
            IsActive = true,
            LastUpdated = DateTime.UtcNow,
            Version = 1
        };

        context.Students.Add(student);
        await context.SaveChangesAsync(ct);
        
        logger.LogInformation("Created student {StudentId} {FullName} {Email}",
            student.Id, request.Name, request.Email);

        return new StudentResponseDto(student.Id, student.RegistrationNumber, student.Name, student.GPA, student.IsActive, student.LastUpdated);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .AnyAsync(s => s.Name == name && s.IsActive, ct);
    }

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search}%";
            query = query.Where(s => EF.Functions.ILike(s.Name, pattern) || EF.Functions.ILike(s.RegistrationNumber, pattern));
        }

        var totalCount = await query.CountAsync(ct);
        query = request.OrderBy switch
        {
            "RegistrationNumber" => request.Descending ? query.OrderByDescending(s => s.RegistrationNumber) : query.OrderBy(s => s.RegistrationNumber),
            "GPA" => request.Descending ? query.OrderByDescending(s => s.GPA) : query.OrderBy(s => s.GPA),
            _ => request.Descending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name)
        };

        var students = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive,
                s.LastUpdated))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto>
        {
            Items = students,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<StudentResponseDto?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out var studentId))
        {
            logger.LogWarning("Invalid student ID format {StudentId}", id);
            return null;
        }

        var student = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if(student is null)
        {
            logger.LogWarning("Student {StudentId} not found", id);
            return null;
        }
        
        return new StudentResponseDto(student.Id, student.RegistrationNumber, student.Name, student.GPA, student.IsActive, student.LastUpdated);
    }

    
    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var studentId))
        {
            logger.LogWarning("Invalid student ID format {StudentId}", id);
            return false;
        }

        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        
        if(student is null)
        {
            logger.LogWarning("Delete failed student {StudentId} not found", id);
            return false;
        }

        student.IsDeleted = true;
        student.IsActive = false;
        student.LastUpdated = DateTime.UtcNow;
        
        context.Students.Update(student);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted student {StudentId}", id);
        return true;
    }
}
    

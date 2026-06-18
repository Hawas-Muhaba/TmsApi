using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class ReportService
{
    private readonly TmsDbContext _dbContext;

    public ReportService(TmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
 
    public Task<int> GetActiveHonorRollStudentCountAsync()
    {
        return _dbContext.Students.CountAsync(s => s.IsActive && s.GPA >= 3.0m);
    }

    public async Task<List<CourseEnrollmentReport>> GetCourseEnrollmentReportAsync()
    {
        return await _dbContext.Courses
            .Select(c => new CourseEnrollmentReport(c.Title, c.Enrollments.Count))
            .OrderByDescending(r => r.EnrolledCount)
            .ToListAsync();
    }

    public async Task<List<CourseGpaReport>> GetCourseGpaReportAsync()
    {
        return await _dbContext.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new CourseGpaReport(g.Key, g.Average(e => e.Student.GPA)))
            .ToListAsync();
    }

    // which students have zero enrollments
    public async Task<List<Student>> GetStudentsWithNoEnrollmentsAsync()
    {
        return await _dbContext.Students
            .Where(s => !s.Enrollments.Any())
            .ToListAsync();
    }
}

public record CourseEnrollmentReport(string Title, int EnrolledCount);
public record CourseGpaReport(string Title, decimal AverageGpa);
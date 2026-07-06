using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        // --- SERVICES SECTION ---
        builder.Services.AddSingleton<EnrollmentWorker>();
        builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<IReportingService, ReportingService>();

       
        builder.Services
            .AddAuthentication("Training")
            .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

        builder.Services.AddOptions<PaymentOptions>()
            .BindConfiguration("Payments")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddProblemDetails(); 
        builder.Services.AddOpenApi();        
        builder.Services.AddAuthorization();

         builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging());

        builder.Services.AddControllers();
        
        var app = builder.Build();
 
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // --- ENDPOINTS SECTION ---

        app.MapGet("/api/assessments/results", () => Results.Ok(new
        {
            courseCode = "CS-101",
            studentId = "S-001",
            letterGrade = "A"
        })).RequireAuthorization();

        app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
        {
            worker.ProcessBatch();
            return Results.Ok("processed");
        });

        
        app.MapGet("/api/error", () =>
        {
            throw new Exception("Simulated failure for ProblemDetails testing");
        });

        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapControllers();

     
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
            dbContext.Database.Migrate();

            if(!dbContext.Students.Any())
            {
                var students = new List<Student>
                {
                    new()
                    {
                        RegistrationNumber = "TMS-2026-0001",
                        Name = "Alice Johnson",
                        GPA = 3.8m,
                        IsActive = true
                    },
                    new()
                    {
                        RegistrationNumber = "TMS-2026-0002",
                        Name = "Bob Smith",
                        GPA = 3.5m,
                        IsActive = true
                    },
                    new()
                    {
                        RegistrationNumber = "TMS-2026-0003",
                        Name = "Charlie Brown",
                        GPA = 3.2m,
                        IsActive = false
                    },
                    new()
                    {
                        RegistrationNumber = "TMS-2026-0004",
                        Name = "Diana Prince",
                        GPA = 3.9m,
                        IsActive = true
                    },
                    new()
                    {
                        RegistrationNumber = "TMS-2026-0005",
                        Name = "Ethan Hunt",
                        GPA = 3.6m,
                        IsActive = true
                    }
                };
                dbContext.AddRange(students);
                 var courses = new List<Course>
                 {
                     new()
                     {
                         Code = "CS-101",
                         Title = "Introdaction to Computer Science",
                         MaxCapacity = 30,
                     },
                        new()
                        {
                            Code = "CS-102",
                            Title = "Data Structures and Algorithms",
                            MaxCapacity = 25,
                        },
                        new()
                        {
                            Code = "CS-103",
                            Title = "Database Systems",
                            MaxCapacity = 20,
                        }
                 };
                dbContext.AddRange(courses);
                dbContext.SaveChanges();
                var enrollments = new List<Enrollment>
                {
                    new()
                    {
                        StudentId = students[0].Id,
                        CourseId = courses[0].Id,
                        Grade = 4.0m
                    },
                    new()
                    {
                        StudentId = students[1].Id,
                        CourseId = courses[1].Id,
                        Grade = 3.5m
                    },
                    new()
                    {
                        StudentId = students[2].Id,
                        CourseId = courses[2].Id,
                        Grade = 3.0m
                    }
                };
                dbContext.AddRange(enrollments);
                dbContext.SaveChanges();
            }
        }

        app.Run();
    }
}

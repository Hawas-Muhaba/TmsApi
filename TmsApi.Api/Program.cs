using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Persistence;
using TmsApi.Filters;
using TmsApi.Middleware;
using Asp.Versioning;
using TmsApi.Application.Services;
using TmsApi.Application.Behaviors;
// add validator assembly
using FluentValidation;
//  add transient pipeline behaviors
using MediatR;
//  add exception handler
using TmsApi.Api.ExceptionHandlers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
// LoggingBehavior FIRST—it must wrap ValidationBehavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddProblemDetails();   
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Host.UseDefaultServiceProvider(o=>{o.ValidateScopes = true; o.ValidateOnBuild = true;});
builder.Services.AddOptions<PaymentOptions>()
        .BindConfiguration("Payments")
        .ValidateDataAnnotations()
        .ValidateOnStart();
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});
builder.Services.AddControllers(options =>
{
options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)   // dev only - prints generated SQL
        .EnableSensitiveDataLogging());  
builder.Services.AddControllers();

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description => 
    description.GroupName == "v1";
});
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description => 
    description.GroupName == "v2";
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.ApiVersionReader = ApiVersionReader.Combine(
    new UrlSegmentApiVersionReader(),
    new HeaderApiVersionReader("X-Api-Version"));

})
.AddApiExplorer(options =>
{
options.GroupNameFormat = "'v'VVV";
options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options
               .AddDocument("v1", "API Version 1.0")
               .AddDocument("v2", "API Version 2.0");
    });
}


app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});
app.MapGet("/api/enrollments/parallel-test", async (EnrollmentWorker worker) =>
{
    // Launch 10 parallel tasks that all call ProcessBatch()
    var tasks = Enumerable.Range(0, 10)
        .Select(_ => Task.Run(() => worker.ProcessBatch()));

    await Task.WhenAll(tasks);

    return Results.Ok("parallel batch processed");
});

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<V1DeprecationMiddleware>();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// app.MapGet("/api/assessments/results", ()=> Results.Ok(new
// {
//     courseCode = "CS_101", studentId="s-001", letterGrade = "A"
// })).RequireAuthorization(); // done in controller side :____
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // applies pending migrations, keeps history intact

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m, IsActive = true }
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to Computer Science", MaxCapacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity = 40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges(); // must save before referencing generated Ids below

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
        };
        context.Enrollments.AddRange(enrollments);

        // Extended entities - same seeding pattern applied to Assessment and Certificate
        var assessments = new List<Assessment>
        {
            new() { Title = "Midterm Exam", MaxScore = 100m, Weight = 0.30m, CourseId = courses[0].Id },
            new() { Title = "Final Project", MaxScore = 100m, Weight = 0.40m, CourseId = courses[0].Id },
            new() { Title = "Problem Set 1", MaxScore = 50m, Weight = 0.10m, CourseId = courses[2].Id }
        };
        context.Assessments.AddRange(assessments);

        var certificates = new List<Certificate>
        {
            new() { SerialNumber = "CERT-2026-0001", StudentId = students[0].Id, CourseId = courses[0].Id }
        };
        context.Certificates.AddRange(certificates);

        context.SaveChanges();
    }
}
if (app.Environment.IsDevelopment())
{
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
await DataSeeder.SeedAsync(context);
}
app.Run();
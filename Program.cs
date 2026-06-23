using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi; // <-- 1. ይህ አዲስ የተጨመረ ነው
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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
        builder.Services.AddControllers();
        builder.Services.AddDbContext<StudentsDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddSingleton<EnrollmentWorker>();
        builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
        builder.Services.AddSingleton<ICourseService, CourseService>();
        builder.Services.AddScoped<IStudentService, StudentService>();

        builder.Services
            .AddAuthentication("Training")
            .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

        builder.Services.AddOptions<PaymentOptions>()
            .BindConfiguration("Payments")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddProblemDetails(); // Exercise 6
        builder.Services.AddOpenApi();        // Exercise 7
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // --- MIDDLEWARE SECTION (ቅደም ተከተሉ ተስተካክሏል) ---

        // 1. የ ProblemDetails ኤረር መያዣዎች ሁሌም መጀመሪያ መሆን አለባቸው
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // 2. Routing እና Security
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

        // Exercise 6: የተስተካከለው የኤረር መሞከሪያ መንገድ (Endpoint)
        app.MapGet("/api/error", () =>
        {
            throw new Exception("Simulated failure for ProblemDetails testing");
        });

        // Exercise 7: የልማት መሳሪያዎች (Dev Tools Environment Toggle)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapControllers();

        // አፕሊኬሽኑን ማስነሻ መስመር
        app.Run();
    }
}

using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


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
builder.Services.AddControllers();

builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// app.MapGet("/api/assessments/results", ()=> Results.Ok(new
// {
//     courseCode = "CS_101", studentId="s-001", letterGrade = "A"
// })).RequireAuthorization(); // done in controller side :____
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});
app.Run();
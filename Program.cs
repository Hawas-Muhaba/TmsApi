using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild= true;
});

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
//services
builder.Services
.AddAuthentication("Training")
.AddScheme<AuthenticationSchemeOptions,
TrainingAuthHandler>("Training", null);

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthorization();

var app = builder.Build();

//middleware order

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results", ()=>Results.Ok(new
{
    courseCode = "CS-101",
    studentId= "S-001",
    letterGrade = "A"
})).RequireAuthorization();
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
worker.ProcessBatch();
return Results.Ok("processed");
});

//protected endpoint



app.Run();
var builder = WebApplication.CreateBuilder(args);

//services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//protected endpoint

app.MapGet("/api/assessments/results", ()=>Results.Ok(new
{
    courseCode = "CS-101",
    studentId= "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();
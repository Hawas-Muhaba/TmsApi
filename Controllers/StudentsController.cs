using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var students = await studentService.GetAllAsync(page, pageSize);
        return Ok(students);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await studentService.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(request.RegistrationNumber, request.Name, request.Gpa, request.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        var updated = await studentService.UpdateAsync(id, request.RegistrationNumber, request.Name, request.Gpa, request.IsActive);
        return updated is not null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateStudentRequest(string RegistrationNumber, string Name, decimal Gpa, bool IsActive = true);
public record UpdateStudentRequest(string RegistrationNumber, string Name, decimal Gpa, bool IsActive);

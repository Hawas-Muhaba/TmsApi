using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await studentService.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(request.FirstName, request.LastName, request.Email);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateStudentRequest request)
    {
        var updated = await studentService.UpdateAsync(id, request.FirstName, request.LastName, request.Email);
        return updated is not null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateStudentRequest(string FirstName, string LastName, string Email);
public record UpdateStudentRequest(string FirstName, string LastName, string Email);

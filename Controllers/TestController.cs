using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;
namespace TmsApi.Controllers;
[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);
        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Execution is triggered here
        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }
    private static bool IsHonorRoll(decimal gpa) => gpa >= 3.5m;

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: running non-translatable LINQ method in the query...");
        try
        {
            var studnets = context.Students.Where(s=> IsHonorRoll(s.GPA)).ToList();
            return Ok(studnets);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPRION CAUGHT: {ex.Message} \n");
            return BadRequest(new {Message = ex.Message});
        }

    }
}



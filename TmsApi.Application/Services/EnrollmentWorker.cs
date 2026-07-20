using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Interfaces;

// bad pattern
// public class EnrollmentWorker
// {
//     private readonly IEnrollmentService _svc;

//     public EnrollmentWorker(IEnrollmentService svc)
//     {
//         _svc = svc;
//     }

//     public void ProcessBatch()
//     {
//         // process it, to show bad captive dependency
//         _svc.EnrollAsync("s-001", "CS_101").GetAwaiter().GetResult();
//     }
// }



// // fixed pattern of scope
namespace TmsApi.Application.Services;

public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        using var scope = scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();    
    }
}
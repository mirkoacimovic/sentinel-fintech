using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Persistance;
using Sentinel.Api.Domain;

namespace Sentinel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestController(SentinelContext context) : ControllerBase
{
    [HttpPost("batch")]
    public async Task<IActionResult> PostBatch([FromBody] List<CostRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("No data provided.");

        foreach (var req in requests)
        {
            // 1. Department
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == req.DepartmentName)
                ?? new Department { Name = req.DepartmentName };

            // 2. Category (Make sure this matches your class name: CostCategory)
            var category = await context.CostCategories
                .FirstOrDefaultAsync(c => c.Name == req.CategoryName)
                ?? new CostCategory { Name = req.CategoryName };

            // 3. Employee
            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Name == req.EmployeeName)
                ?? new Employee { Name = req.EmployeeName, Department = dept };

            // 4. Cost
            var cost = new Cost
            {
                Description = req.Description,
                Amount = req.Amount,
                ProcessedAt = req.ProcessedAt,
                Employee = employee,
                Category = category
            };
        }

        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(PostBatch), new { count = requests.Count });
    }
}

// The DTO (Data Transfer Object) for the incoming JSON
public record CostRequest(
    string EmployeeName,
    string DepartmentName,
    string CategoryName,
    string Description,
    decimal Amount,
    DateTime ProcessedAt);
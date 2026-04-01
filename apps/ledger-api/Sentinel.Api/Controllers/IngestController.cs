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

        // Define our Tenant ID to satisfy the NOT NULL constraint
        const int tenantId = 1;

        foreach (var req in requests)
        {
            // 1. Ensure Department Exists
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == req.DepartmentName)
                ?? new Department
                {
                    Name = req.DepartmentName,
                    CompanyId = tenantId // Added to fix constraint
                };

            if (dept.Id == 0) context.Departments.Add(dept);

            // 2. Ensure Employee Exists
            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Name == req.EmployeeName)
                ?? new Employee
                {
                    Name = req.EmployeeName,
                    Department = dept,
                    CompanyId = tenantId // Added to fix constraint
                };

            if (employee.Id == 0) context.Employees.Add(employee);

            // 3. Ensure Category Exists
            var category = await context.CostCategories
                .FirstOrDefaultAsync(c => c.Name == req.CategoryName)
                ?? new CostCategory
                {
                    Name = req.CategoryName,
                    CompanyId = tenantId // Added to fix constraint
                };

            if (category.Id == 0) context.CostCategories.Add(category);

            // 4. Create the Cost Record
            var cost = new Cost
            {
                Description = req.Description,
                Amount = req.Amount,
                ProcessedAt = req.ProcessedAt,
                Employee = employee,
                Category = category,
                CompanyId = tenantId // Added to fix constraint
            };

            context.Costs.Add(cost);
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
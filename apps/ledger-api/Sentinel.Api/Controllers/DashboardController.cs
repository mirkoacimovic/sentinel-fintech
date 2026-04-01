using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Persistance;

namespace Sentinel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(SentinelContext context) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalSpend = await context.Costs.SumAsync(c => c.Amount);
        var employeeCount = await context.Employees.CountAsync();
        var latestCosts = await context.Costs
            .OrderByDescending(c => c.ProcessedAt)
            .Take(5)
            .Select(c => new { c.Description, c.Amount, c.ProcessedAt })
            .ToListAsync();

        return Ok(new
        {
            TotalSpend = totalSpend,
            EmployeeCount = employeeCount,
            Recent = latestCosts
        });
    }
}

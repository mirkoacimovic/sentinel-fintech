using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Domain;
using Sentinel.Api.Persistance;
using Sentinel.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// The 'DefaultConnection' string will now be overridden by the 
// ConnectionStrings__DefaultConnection env variable in docker-compose.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SentinelContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
builder.Services.AddHostedService<AuditConsumer>();

var app = builder.Build();

app.MapGet("/", () => "Sentinel Ledger API is Online");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SentinelContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("--- Database initialized successfully ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--- Database Error: {ex.Message} ---");
    }
}

app.Run();
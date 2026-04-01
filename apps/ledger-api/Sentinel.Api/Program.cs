using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Domain;
using Sentinel.Api.Persistance;

var builder = WebApplication.CreateBuilder(args);

// 1. Pull the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Register the Context
builder.Services.AddDbContext<SentinelContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

// Temporary seed logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SentinelContext>();
    if (!db.Companies.Any(c => c.Id == 1))
    {
        db.Companies.Add(new Company { Id = 1, Name = "Sentinel Core", Address = "Firestone Road FE443D" });
        db.SaveChanges();
    }
}

// 3. Simple Health Check (Internal testing)
app.MapGet("/", () => "Sentinel Ledger API is Online");

app.MapControllers();
app.Run();
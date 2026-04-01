using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Persistance;

var builder = WebApplication.CreateBuilder(args);

// 1. Pull the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Register the Context
builder.Services.AddDbContext<SentinelContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

// 3. Simple Health Check (Internal testing)
app.MapGet("/", () => "Sentinel Ledger API is Online");

app.MapControllers();
app.Run();
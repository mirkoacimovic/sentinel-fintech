using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Persistance;
using Sentinel.Api.Domain;
using Sentinel.Api.Controllers;

namespace Sentinel.Api.Workers;

public class AuditConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly int _tenantId;

    public AuditConsumer(IServiceProvider serviceProvider, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _config = config;

        // Reading from Environment/Appsettings
        _hostname = _config["RabbitMQ:Host"] ?? "rabbitmq";
        _queueName = _config["RabbitMQ:Queue"] ?? "audit_queue";
        _tenantId = int.TryParse(_config["TENANT_ID"], out var id) ? id : 1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IConnection? connection = null;

        // --- RESILIENCE: Retry loop for RabbitMQ Startup ---
        for (int i = 0; i < 10 && connection == null && !stoppingToken.IsCancellationRequested; i++)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = _hostname };
                connection = await factory.CreateConnectionAsync();
            }
            catch
            {
                Console.WriteLine($"--- AuditConsumer: RabbitMQ not ready (Attempt {i + 1}/10). Waiting 5s... ---");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection == null) return;

        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<SentinelContext>();
                    var requests = JsonSerializer.Deserialize<List<CostRequest>>(message);

                    if (requests != null && requests.Count > 0)
                    {
                        foreach (var req in requests)
                        {
                            // Use the Dynamic TenantId from config!
                            var dept = await context.Departments
                                .FirstOrDefaultAsync(d => d.Name == req.DepartmentName)
                                ?? new Department { Name = req.DepartmentName, CompanyId = _tenantId };
                            if (dept.Id == 0) context.Departments.Add(dept);

                            var employee = await context.Employees
                                .FirstOrDefaultAsync(e => e.Name == req.EmployeeName)
                                ?? new Employee { Name = req.EmployeeName, Department = dept, CompanyId = _tenantId };
                            if (employee.Id == 0) context.Employees.Add(employee);

                            var category = await context.CostCategories
                                .FirstOrDefaultAsync(c => c.Name == req.CategoryName)
                                ?? new CostCategory { Name = req.CategoryName, CompanyId = _tenantId };
                            if (category.Id == 0) context.CostCategories.Add(category);

                            context.Costs.Add(new Cost
                            {
                                Description = req.Description,
                                Amount = req.Amount,
                                ProcessedAt = req.ProcessedAt,
                                Employee = employee,
                                Category = category,
                                CompanyId = _tenantId
                            });
                        }

                        await context.SaveChangesAsync();
                        Console.WriteLine($" [x] AuditConsumer (Tenant:{_tenantId}): Processed {requests.Count} records.");
                    }
                }
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Error in AuditConsumer: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
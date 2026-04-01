namespace Sentinel.Api.Domain
{
    public record Cost(
    int Id,
    int DepartmentId,
    int CategoryId,
    int EmployeeId,
    string Description,
    decimal Amount,
    DateTime ProcessedAt);
}

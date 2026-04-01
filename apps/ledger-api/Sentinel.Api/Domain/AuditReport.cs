namespace Sentinel.Api.Domain
{
    public record AuditReport(
    int Id,
    int CompanyId,
    decimal InputCost,
    decimal ReducedCost,
    List<int> OverreachingEmployeeIds);
}

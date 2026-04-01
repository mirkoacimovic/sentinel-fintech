namespace Sentinel.Api.Domain
{
    public record Employee(int Id, int DepartmentId, string Name, string LastName, decimal MonthlyPaycheck);
}

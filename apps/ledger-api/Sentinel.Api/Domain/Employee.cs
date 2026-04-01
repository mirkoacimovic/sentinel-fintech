namespace Sentinel.Api.Domain
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;
    }
}

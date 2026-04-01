namespace Sentinel.Api.Domain
{
    public class Cost
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ProcessedAt { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int CategoryId { get; set; }
        public CostCategory Category { get; set; } = null!; // Changed to match your Error (CostCategory)
    }
}

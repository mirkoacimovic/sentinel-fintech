namespace Sentinel.Api.Domain
{
    public class CostCategory
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

namespace FarmAPI.Models
{
    public class CreateInputCatalogDto
    {
        public required string Type { get; set; }
        public required string Name { get; set; }
        public string? UnitType { get; set; }
        public double? QuantityPerUnit { get; set; }
        public string? DisplayUnit { get; set; }
    }
}

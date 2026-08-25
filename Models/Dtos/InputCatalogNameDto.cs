namespace FarmAPI.Models
{
    public class InputCatalogNameDto
    {
        public required string Name { get; set; }
        public string? UnitType { get; set; }
        public double? QuantityPerUnit { get; set; }
    }
}

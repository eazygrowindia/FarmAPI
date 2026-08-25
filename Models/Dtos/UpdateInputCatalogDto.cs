namespace FarmAPI.Models
{
    public class UpdateInputCatalogDto
    {
        public required string Type { get; set; }
        public required string OldName { get; set; }
        public required string NewName { get; set; }
        public string? UnitType { get; set; }
        public double? QuantityPerUnit { get; set; }
    }
}

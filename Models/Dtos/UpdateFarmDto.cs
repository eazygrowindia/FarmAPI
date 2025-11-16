namespace FarmAPI.Models.Dtos
{
    // Update DTO intentionally excludes the database-generated `Id` and the stable `FarmId`.
    // Use the route `id` (FarmId) when calling update endpoints to identify the resource.
    public class UpdateFarmDto
    {
        public string FarmName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string GPSLocation { get; set; } = null!;
        public string SizeInSqMtrs { get; set; } = null!;
    }
}
namespace FarmAPI.Models.Dtos
{
    public class CreateFarmDto
    {
        public string FarmName { get; set; } = null!;
        public string FarmId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string GPSLocation { get; set; } = null!;
        public string SizeInSqMtrs { get; set; } = null!;
    }
}
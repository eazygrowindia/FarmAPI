using System.Collections.Generic;

namespace FarmAPI.Models.Dtos
{
    // Update DTO intentionally excludes the database-generated `Id`.
    public class UpdateMaintainerDto
    {
        public string MaintainerName { get; set; } = null!;
        public string ContactNumber { get; set; } = null!;
        public string AlternateContactNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string IdentityProofDocument { get; set; } = null!;
        public string IdentityProofNumber { get; set; } = null!;
        public List<string> FarmsMaintained { get; set; } = new List<string>();
        public string Role { get; set; } = null!;
        public string SystemStatus { get; set; } = null!;
    }
}
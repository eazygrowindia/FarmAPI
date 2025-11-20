using System.Collections.Generic;

namespace FarmAPI.Models.Dtos
{
    public class CreateOwnerDto
    {
        public string OwnerId { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string IdentityProofDocument { get; set; } = null!;
        public string IdentityProofNumber { get; set; } = null!;
        public string ContactNumber { get; set; } = null!;
        public string AlternateContactNumber { get; set; }
        public string? EmailId { get; set; }
        public string Address { get; set; } = null!;
        public List<string> FarmsOwned { get; set; } = new List<string>();
    }
}
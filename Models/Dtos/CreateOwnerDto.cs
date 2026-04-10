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
        public List<HealthCheck> HealthChecks { get; set; }
        public List<string> FarmsOwned { get; set; } = new List<string>();
        public List<string> Maintainers { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System.Collections.Generic;

namespace FarmAPI.Models.Dtos
{
    public class CreateMaintainerDto
    {
        public string MaintainerId { get; set; } = null!;
        public string FarmOwnerId { get; set; } = null!;
        public string MaintainerName { get; set; } = null!;
        public string ContactNumber { get; set; } = null!;
        public string AlternateContactNumber { get; set; }
        public string Education { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string IdentityProofDocument { get; set; } = null!;
        public string IdentityProofNumber { get; set; } = null!;
        public string TrainingCertificateUrl { get; set; }
        public List<HealthCheck> HealthChecks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
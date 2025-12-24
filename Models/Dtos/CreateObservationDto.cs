using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FarmAPI.Models.Dtos
{
    public class CreateObservationDto
    {
        //public string ObservationId { get; set; }
        public string CropId { get; set; }
        public string ObservationType { get; set; }
        public string? Message { get; set; }
        public string? Photo { get; set; }
        public string? VoiceNote { get; set; }
    }
}
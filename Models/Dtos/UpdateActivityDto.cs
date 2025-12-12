using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FarmAPI.Models.Dtos
{
    public class UpdateActivityDto
    {
        public string CropId { get; set; }
        public string ActivityType { get; set; }
        public string Message { get; set; }
    }
}
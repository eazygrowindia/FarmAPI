using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FarmAPI.Models.Dtos
{
    public class UpdateUserDto
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string? Email { get; set; }
        //public string Password { get; set; }
        public List<string>? Roles { get; set; } = new List<string>([Models.UserRoles.UNKNOWN.ToString()]);
        //public string SystemStatus { get; set; }
    }
}
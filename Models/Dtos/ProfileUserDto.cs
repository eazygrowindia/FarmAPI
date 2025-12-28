using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FarmAPI.Models.Dtos
{
    public class ProfileUserDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string? Email { get; set; }
        //public string Password { get; set; }
        public List<string>? Roles { get; set; }
        //public string SystemStatus { get; set; }
    }

    public class UpdateProfileUserDto
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
}
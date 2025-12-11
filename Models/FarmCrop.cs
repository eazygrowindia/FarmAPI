using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class FarmCrop
    {
        public FarmPartial farm;
        public CropPartial crop;
    }
}
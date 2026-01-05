using MongoDB.Driver;

namespace FarmAPI.Models
{
    public interface IDatabaseFactory
    {
        IMongoDatabase FarmGrow { get; }
        IMongoDatabase AddressLoader { get; }
    }
}

using MongoDB.Driver;

namespace FarmAPI.Models
{
    public class DatabaseFactory : IDatabaseFactory
    {
        public IMongoDatabase FarmGrow { get; }
        public IMongoDatabase AddressLoader { get; }

        public DatabaseFactory([FromKeyedServices("FarmGrowDB")] IMongoDatabase farmDb,
                              [FromKeyedServices("AddressLoaderDB")] IMongoDatabase addressDb)
        {
            FarmGrow = farmDb;
            AddressLoader = addressDb;
        }
    }

}

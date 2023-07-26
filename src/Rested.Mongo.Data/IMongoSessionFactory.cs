using MongoDB.Driver;

namespace Rested.Mongo.Data
{
    public interface IMongoSessionFactory
    {
        Task<IClientSessionHandle> CreateAsync();
    }
}

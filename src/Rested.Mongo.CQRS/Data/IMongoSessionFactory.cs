using MongoDB.Driver;

namespace Rested.Mongo.CQRS.Data
{
    public interface IMongoSessionFactory
    {
        Task<IClientSessionHandle> CreateAsync();
    }
}

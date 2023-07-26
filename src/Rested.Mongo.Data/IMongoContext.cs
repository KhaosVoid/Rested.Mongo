using MongoDB.Driver;

namespace Rested.Mongo.Data
{
    public interface IMongoContext
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
        IMongoRepositoryFactory RepositoryFactory { get; }
        IMongoSessionFactory SessionFactory { get; }
        ICollectionNameService CollectionNameServices { get; }
    }
}

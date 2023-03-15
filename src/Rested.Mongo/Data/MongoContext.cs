using MongoDB.Driver;

namespace Rested.Mongo.Data
{
    public class MongoContext : IMongoContext
    {
        #region Properties

        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
        public IMongoSessionFactory SessionFactory { get; }
        public IMongoRepositoryFactory RepositoryFactory { get; }
        public ICollectionNameService CollectionNameServices { get; }

        #endregion Properties

        #region Ctor

        public MongoContext(IMongoClient client,
            IMongoDatabase database,
            IMongoSessionFactory sessionFactory,
            IMongoRepositoryFactory repositoryFactory,
            ICollectionNameService collectionNameService)
        {
            Client = client;
            Database = database;
            SessionFactory = sessionFactory;
            RepositoryFactory = repositoryFactory;
            CollectionNameServices = collectionNameService;
        }

        #endregion Ctor
    }
}

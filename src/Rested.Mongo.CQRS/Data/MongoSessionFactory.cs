using MongoDB.Driver;

namespace Rested.Mongo.CQRS.Data
{
    public sealed class MongoSessionFactory : IMongoSessionFactory
    {
        #region Members

        private readonly IMongoClient _mongoClient;

        #endregion Members

        #region Ctor

        public MongoSessionFactory(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        #endregion Ctor

        #region Methods

        public Task<IClientSessionHandle> CreateAsync()
        {
            return _mongoClient.StartSessionAsync();
        }

        #endregion Methods
    }
}

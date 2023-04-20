using Microsoft.Extensions.DependencyInjection;
using Rested.Core.CQRS.Data;

namespace Rested.Mongo.CQRS.Data
{
    public class MongoRepositoryFactory : IMongoRepositoryFactory
    {
        #region Properties

        protected IServiceProvider ServiceProvider { get; }

        #endregion Properties

        #region Methods

        public MongoRepositoryFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IMongoRepository<TData> Create<TData>() where TData : IData
        {
            return ServiceProvider.GetRequiredService<IMongoRepository<TData>>();
        }

        #endregion Methods
    }
}

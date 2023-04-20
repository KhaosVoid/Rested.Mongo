using Rested.Core.CQRS.Data;

namespace Rested.Mongo.CQRS.Data
{
    public interface IMongoRepositoryFactory
    {
        IMongoRepository<TData> Create<TData>() where TData : IData;
    }
}

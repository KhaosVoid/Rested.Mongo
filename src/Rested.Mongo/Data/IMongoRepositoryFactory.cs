using Rested.Core.Data;

namespace Rested.Mongo.Data
{
    public interface IMongoRepositoryFactory
    {
        IMongoRepository<TData> Create<TData>() where TData : IData;
    }
}

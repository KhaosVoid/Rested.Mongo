using Rested.Core.CQRS.Data;

namespace Rested.Mongo.CQRS.Data
{
    public interface ICollectionNameService
    {
        string GetCollectionName<TData>() where TData : IData;
    }
}

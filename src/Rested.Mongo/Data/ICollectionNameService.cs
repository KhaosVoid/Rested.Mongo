using Rested.Core.Data;

namespace Rested.Mongo.Data
{
    public interface ICollectionNameService
    {
        string GetCollectionName<TData>() where TData : IData;
    }
}

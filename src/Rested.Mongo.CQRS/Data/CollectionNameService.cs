using Rested.Core.CQRS.Data;

namespace Rested.Mongo.CQRS.Data
{
    public class CollectionNameService : ICollectionNameService
    {
        public string GetCollectionName<TData>() where TData : IData
        {
            return typeof(TData).GetCollectionName();
        }
    }
}

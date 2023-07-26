using Rested.Core.Data;

namespace Rested.Mongo.Data
{
    public class CollectionNameService : ICollectionNameService
    {
        public string GetCollectionName<TData>() where TData : IData
        {
            return typeof(TData).GetCollectionName();
        }
    }
}

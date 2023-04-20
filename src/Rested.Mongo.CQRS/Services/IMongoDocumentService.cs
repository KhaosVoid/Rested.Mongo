using Rested.Core.CQRS.Data;
using Rested.Core.CQRS.Services;
using Rested.Mongo.CQRS.Data;

namespace Rested.Mongo.Services
{
    public interface IMongoDocumentService<TData> : IDocumentService<TData, MongoDocument<TData>>
        where TData : IData
    {

    }
}

using MongoDB.Driver;
using Rested.Core.CQRS.Data;

namespace Rested.Mongo.CQRS.Data
{
    public interface IMongoDocumentAuditingService : IDocumentAuditingService
    {
        void SetDocumentAuditingInformation<TData>(MongoDocument<TData> document, bool isUpdate = false)
            where TData : IData;

        UpdateDefinition<MongoDocument<TData>> UpdateDocumentAuditingInformation<TData>(UpdateDefinition<MongoDocument<TData>> updateDefinition = null)
            where TData : IData;
    }
}

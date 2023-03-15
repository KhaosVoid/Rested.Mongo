using MongoDB.Driver;
using Rested.Core.Data;

namespace Rested.Mongo.Data
{
    public interface IMongoDocumentAuditingService : IDocumentAuditingService
    {
        void SetDocumentAuditingInformation<TData>(MongoDocument<TData> document, bool isUpdate = false)
            where TData : IData;

        UpdateDefinition<MongoDocument<TData>> UpdateDocumentAuditingInformation<TData>(UpdateDefinition<MongoDocument<TData>> updateDefinition)
            where TData : IData;
    }
}

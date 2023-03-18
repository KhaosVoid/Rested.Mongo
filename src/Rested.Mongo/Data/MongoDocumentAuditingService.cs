using MongoDB.Driver;
using Rested.Core.Data;

namespace Rested.Mongo.Data
{
    public class MongoDocumentAuditingService : IMongoDocumentAuditingService
    {
        public UpdateDefinition<MongoDocument<TData>> UpdateDocumentAuditingInformation<TData>(UpdateDefinition<MongoDocument<TData>> updateDefinition = null)
            where TData : IData
        {
            var documentAuditingUpdateDefinition = Builders<MongoDocument<TData>>
                .Update
                .Inc(x => x.UpdateVersion, 1UL)
                .Set(x => x.UpdateDateTime, DateTime.UtcNow);

            if (updateDefinition is null)
                return documentAuditingUpdateDefinition;

            return Builders<MongoDocument<TData>>.Update.Combine(updateDefinition, documentAuditingUpdateDefinition);
        }

        public void SetDocumentAuditingInformation<TData>(MongoDocument<TData> document, bool isUpdate = false)
            where TData : IData
        {
            SetDocumentAuditingInformation<TData, MongoDocument<TData>>(document, isUpdate);
        }

        public void SetDocumentAuditingInformation<TData, TDocument>(TDocument document, bool isUpdate = false)
            where TData : IData
            where TDocument : IDocument<TData>
        {
            if (isUpdate)
            {
                document.UpdateDateTime = DateTime.UtcNow;
                document.UpdateVersion++;
            }

            else
            {
                document.CreateDateTime = DateTime.UtcNow;
                document.UpdateDateTime = DateTime.UtcNow;
            }
        }
    }
}

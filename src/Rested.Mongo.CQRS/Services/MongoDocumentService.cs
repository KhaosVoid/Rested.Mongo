using Rested.Core.CQRS.Data;
using Rested.Core.CQRS.Services;
using Rested.Mongo.CQRS.Data;

namespace Rested.Mongo.Services
{
    public abstract class MongoDocumentService<TData> : DocumentService<TData, MongoDocument<TData>>, IMongoDocumentService<TData>
        where TData : IData
    {
        #region Ctor

        public MongoDocumentService(HttpClient httpClient) : base(httpClient)
        {

        }

        #endregion Ctor
    }
}

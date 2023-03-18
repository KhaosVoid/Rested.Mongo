using MongoDB.Driver;
using Rested.Core.Data;
using System.Linq.Expressions;

namespace Rested.Mongo.Data
{
    public interface IMongoRepository<TData> where TData : IData
    {
        #region Properties

        IMongoCollection<MongoDocument<TData>> Collection { get; }

        #endregion Properties

        #region Methods

        Task<bool> DocumentExistsAsync(Expression<Func<MongoDocument<TData>, bool>> predicate);
        Task<bool> DocumentExistsAsync(FilterDefinition<MongoDocument<TData>> filterDefinition);
        Task<MongoDocument<TData>> GetDocumentAsync(Guid id);
        Task<MongoDocument<TData>> GetDocumentAsync(Expression<Func<MongoDocument<TData>, bool>> predicate);
        Task<MongoDocument<TData>> GetDocumentAsync(FilterDefinition<MongoDocument<TData>> filterDefinition);
        Task<TProjection> GetProjectionAsync<TProjection>(Expression<Func<MongoDocument<TData>, bool>> predicate, ProjectionDefinition<MongoDocument<TData>, TProjection> projectionDefinition);
        Task<TProjection> GetProjectionAsync<TProjection>(FilterDefinition<MongoDocument<TData>> filterDefinition, ProjectionDefinition<MongoDocument<TData>, TProjection> projectionDefinition);
        Task<IEnumerable<MongoDocument<TData>>> FindDocumentsAsync(Expression<Func<MongoDocument<TData>, bool>> predicate);
        Task<IEnumerable<MongoDocument<TData>>> FindDocumentsAsync(FilterDefinition<MongoDocument<TData>> filterDefinition);
        Task<IEnumerable<TProjection>> FindProjectionsAsync<TProjection>(FilterDefinition<MongoDocument<TData>> filterDefinition, ProjectionDefinition<MongoDocument<TData>, TProjection> projectionDefinition);
        Task InsertDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool setDocumentAuditingInformation = false);
        Task InsertDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool setDocumentAuditingInformation = false);
        Task UpdateDocumentAsync(Guid id, ulong updateVersion, UpdateDefinition<MongoDocument<TData>> updateDefinition, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task UpdateDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task UpdateDocumentDataAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task UpdateDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task UpdateDocumentsDataAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task PatchDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task PatchDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false);
        Task DeleteDocumentAsync(Guid id, ulong updateVersion, IClientSessionHandle? session = null);
        Task DeleteDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null);
        Task DeleteDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null);

        #endregion Methods
    }
}

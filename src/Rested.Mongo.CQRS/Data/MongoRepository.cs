using MongoDB.Driver;
using Rested.Core.CQRS.Data;
using System.Collections;
using System.Linq.Expressions;

namespace Rested.Mongo.CQRS.Data
{
    public class MongoRepository<TData> : IMongoRepository<TData> where TData : IData
    {
        #region Properties

        public IMongoCollection<MongoDocument<TData>> Collection { get; protected set; }

        #endregion Properties

        #region Members

        protected readonly IMongoDocumentAuditingService _mongoDocumentAuditingService;
        protected readonly ICollectionNameService _collectionNameService;

        protected string _collectionName;

        #endregion Members

        #region Ctor

        public MongoRepository(
            IMongoDocumentAuditingService mongoDocumentAuditingService,
            IMongoDatabase mongoDatabase,
            ICollectionNameService collectionNameService)
        {
            _mongoDocumentAuditingService = mongoDocumentAuditingService;
            _collectionNameService = collectionNameService;
            _collectionName = _collectionNameService.GetCollectionName<TData>();

            Collection = mongoDatabase.GetCollection<MongoDocument<TData>>(_collectionName);
        }

        #endregion Ctor

        #region Methods

        public async Task<bool> DocumentExistsAsync(Expression<Func<MongoDocument<TData>, bool>> predicate)
        {
            return await Collection.Find(predicate).AnyAsync();
        }

        public async Task<bool> DocumentExistsAsync(FilterDefinition<MongoDocument<TData>> filterDefinition)
        {
            return await Collection.Find(filterDefinition).AnyAsync();
        }

        public async Task<MongoDocument<TData>> GetDocumentAsync(Guid id)
        {
            return await GetDocumentAsync(x => x.Id == id);
        }

        public async Task<MongoDocument<TData>> GetDocumentAsync(Expression<Func<MongoDocument<TData>, bool>> predicate)
        {
            return await Collection.Find(predicate).SingleOrDefaultAsync();
        }

        public async Task<MongoDocument<TData>> GetDocumentAsync(FilterDefinition<MongoDocument<TData>> filterDefinition)
        {
            return await Collection.Find(filterDefinition).SingleOrDefaultAsync();
        }

        public async Task<TProjection> GetProjectionAsync<TProjection>(Expression<Func<MongoDocument<TData>, bool>> predicate, ProjectionDefinition<MongoDocument<TData>, TProjection> projectionDefinition)
        {
            return await Collection.Find(predicate).Project(projectionDefinition).SingleOrDefaultAsync();
        }

        public async Task<TProjection> GetProjectionAsync<TProjection>(FilterDefinition<MongoDocument<TData>> filterDefinition, ProjectionDefinition<MongoDocument<TData>, TProjection> projectionDefinition)
        {
            return await Collection.Find(filterDefinition).Project(projectionDefinition).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MongoDocument<TData>>> FindDocumentsAsync(Expression<Func<MongoDocument<TData>, bool>> predicate)
        {
            return await Collection.Find(predicate).ToListAsync();
        }

        public async Task<IEnumerable<MongoDocument<TData>>> FindDocumentsAsync(FilterDefinition<MongoDocument<TData>> filterDefinition)
        {
            return await Collection.Find(filterDefinition).ToListAsync();
        }

        public async Task<IEnumerable<TProjection>> FindProjectionsAsync<TProjection>(FilterDefinition<MongoDocument<TData>> filterDefinition, ProjectionDefinition<MongoDocument<TData>, TProjection> projectionDefinition)
        {
            return await Collection.Find(filterDefinition).Project(projectionDefinition).ToListAsync();
        }

        public async Task InsertDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool setDocumentAuditingInformation = false)
        {
            if (setDocumentAuditingInformation)
                _mongoDocumentAuditingService.SetDocumentAuditingInformation(document);

            if (session == null)
                await Collection.InsertOneAsync(document);

            else
                await Collection.InsertOneAsync(session, document);
        }

        public async Task InsertDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool setDocumentAuditingInformation = false)
        {
            var mongoExceptions = new List<MongoMultipleExceptionDetail>();

            foreach (MongoDocument<TData> document in documents)
            {
                try
                {
                    await InsertDocumentAsync(document, session, setDocumentAuditingInformation);
                }
                catch (Exception exception)
                {
                    mongoExceptions.Add(new MongoMultipleExceptionDetail(
                        collectionName: Collection.CollectionNamespace.CollectionName,
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        exception: exception));
                }
            }

            if (mongoExceptions?.Any() ?? false)
            {
                throw new MongoMultipleException(
                    message: "Multiple errors have occurred during the bulk insert operation.",
                    mongoExceptions: mongoExceptions);
            }
        }

        public async Task UpdateDocumentAsync(Guid id, ulong updateVersion, UpdateDefinition<MongoDocument<TData>> updateDefinition, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            if (updateDocumentAuditingInformation)
                updateDefinition = _mongoDocumentAuditingService.UpdateDocumentAuditingInformation(updateDefinition);

            Expression<Func<MongoDocument<TData>, bool>> updateFilterExpression = (x) => x.Id == id && x.UpdateVersion.Equals(updateVersion);

            var updateResult = session != null ?
                await Collection.UpdateOneAsync(session, updateFilterExpression, updateDefinition) :
                await Collection.UpdateOneAsync(updateFilterExpression, updateDefinition);

            if (updateResult.ModifiedCount == 0)
            {
                if (await DocumentExistsAsync(x => x.Id == id))
                    throw new MongoConcurrencyException(id);

                throw new DocumentNotFoundException(_collectionName, id);
            }
        }

        public async Task UpdateDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            if (updateDocumentAuditingInformation)
                _mongoDocumentAuditingService.SetDocumentAuditingInformation(document, true);

            Expression<Func<MongoDocument<TData>, bool>> updateFilterExpression = (x) => x.Id == document.Id && x.UpdateVersion.Equals(document.UpdateVersion);

            var replaceOneResult = session != null ?
                await Collection.ReplaceOneAsync(session, updateFilterExpression, document) :
                await Collection.ReplaceOneAsync(updateFilterExpression, document);

            if (replaceOneResult.ModifiedCount == 0)
            {
                document.UpdateVersion--;

                if (await DocumentExistsAsync(x => x.Id == document.Id))
                    throw new MongoConcurrencyException(document.Id);

                throw new DocumentNotFoundException(_collectionName, document.Id);
            }
        }

        public async Task UpdateDocumentDataAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            var updateDefinition = Builders<MongoDocument<TData>>
                .Update
                .Set(
                    field: f => f.Data,
                    value: document.Data);

            await UpdateDocumentAsync(
                id: document.Id,
                updateVersion: document.UpdateVersion,
                updateDefinition: updateDefinition,
                session: session,
                updateDocumentAuditingInformation: updateDocumentAuditingInformation);
        }

        public async Task UpdateDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            var mongoExceptions = new List<MongoMultipleExceptionDetail>();

            foreach (var document in documents)
            {
                try
                {
                    await UpdateDocumentAsync(document, session, updateDocumentAuditingInformation);
                }
                catch (Exception ex)
                {
                    mongoExceptions.Add(new MongoMultipleExceptionDetail(
                        collectionName: Collection.CollectionNamespace.CollectionName,
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        exception: ex));
                }
            }

            if (mongoExceptions?.Any() ?? false)
            {
                throw new MongoMultipleException(
                    message: "Multiple errors have occurred during the bulk update operation.",
                    mongoExceptions: mongoExceptions);
            }
        }

        public async Task UpdateDocumentsDataAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            var mongoExceptions = new List<MongoMultipleExceptionDetail>();

            foreach (MongoDocument<TData> document in documents)
            {
                try
                {
                    var updateDefinition = Builders<MongoDocument<TData>>
                        .Update
                        .Set(
                            field: f => f.Data,
                            value: document.Data);

                    await UpdateDocumentAsync(
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        updateDefinition: updateDefinition,
                        session: session,
                        updateDocumentAuditingInformation: updateDocumentAuditingInformation);
                }
                catch (Exception exception)
                {
                    mongoExceptions.Add(new MongoMultipleExceptionDetail(
                        collectionName: Collection.CollectionNamespace.CollectionName,
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        exception: exception));
                }
            }

            if (mongoExceptions?.Any() ?? false)
            {
                throw new MongoMultipleException(
                    message: "Multiple errors have occurred during the bulk update operation.",
                    mongoExceptions: mongoExceptions);
            }
        }

        public async Task PatchDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            Expression<Func<MongoDocument<TData>, bool>> patchFilterExpression =
                (x) => x.Id.Equals(document.Id) && x.UpdateVersion.Equals(document.UpdateVersion);

            var patchDefinition = CreatePatchDefinition(
                data: document,
                updateDefinition: Builders<MongoDocument<TData>>.Update.Set(
                    field: f => f.Id,
                    value: document.Id));

            if (updateDocumentAuditingInformation)
            {
                patchDefinition = _mongoDocumentAuditingService.UpdateDocumentAuditingInformation(patchDefinition);
            }

            else
            {
                if (document.UpdateDateTime is not null)
                {
                    patchDefinition = patchDefinition
                        .Set(
                            field: f => f.UpdateDateTime,
                            value: document.UpdateDateTime);
                }

                if (document.UpdateUser is not null)
                {
                    patchDefinition = patchDefinition
                        .Set(
                            field: f => f.UpdateUser,
                            value: document.UpdateUser);
                }

                patchDefinition = patchDefinition
                    .Set(
                        field: f => f.UpdateVersion,
                        value: document.UpdateVersion);
            }

            var patchResult = session is not null ?
                await Collection.UpdateOneAsync(session, patchFilterExpression, patchDefinition) :
                await Collection.UpdateOneAsync(patchFilterExpression, patchDefinition);

            if (patchResult.ModifiedCount is 0)
            {
                if (await DocumentExistsAsync(x => x.Id == document.Id))
                    throw new MongoConcurrencyException(document.Id);

                throw new DocumentNotFoundException(_collectionName, document.Id);
            }
        }

        public async Task PatchDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            var mongoExceptions = new List<MongoMultipleExceptionDetail>();

            foreach (var document in documents)
            {
                try
                {
                    await PatchDocumentAsync(document, session, updateDocumentAuditingInformation);
                }
                catch (Exception ex)
                {
                    mongoExceptions.Add(new MongoMultipleExceptionDetail(
                        collectionName: Collection.CollectionNamespace.CollectionName,
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        exception: ex));
                }
            }

            if (mongoExceptions?.Any() ?? false)
            {
                throw new MongoMultipleException(
                    message: "Multiple errors have occurred during the bulk patch operation.",
                    mongoExceptions: mongoExceptions);
            }
        }

        private UpdateDefinition<MongoDocument<TData>> CreatePatchDefinition<T>(T data, UpdateDefinition<MongoDocument<TData>> updateDefinition, string propertyPath = "")
        {
            foreach (var property in data?.GetType().GetProperties())
            {
                string currentPropertyPath = string.IsNullOrEmpty(propertyPath) ?
                    property.Name.ToCamelCase() :
                    $"{propertyPath}.{property.Name.ToCamelCase()}";

                var propertyValue = data?.GetType()?.GetProperty(property.Name)?.GetValue(data);

                if (propertyValue is null)
                    continue;

                if (currentPropertyPath == nameof(IIdentifiable.Id).ToCamelCase())
                    continue;

                //if (currentPropertyPath == nameof(IDocument<TData>.CreateDateTime).ToCamelCase())
                //    continue;

                //if (currentPropertyPath == nameof(IDocument<TData>.CreateUser).ToCamelCase())
                //    continue;

                if (currentPropertyPath == nameof(IPersistedDocument.UpdateDateTime).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IPersistedDocument.UpdateUser).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IPersistedDocument.UpdateVersion).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IDocument<TData>.ETag).ToCamelCase())
                    continue;

                if (property.PropertyType.IsClass && !property.PropertyType.IsPrimitive &&
                    property.PropertyType != typeof(object) && property.PropertyType != typeof(string))
                {
                    if (property.PropertyType.IsArray)
                    {
                        var propertyValueArray = propertyValue as Array;

                        for (int i = 0; i < propertyValueArray?.Length; i++)
                        {
                            updateDefinition = updateDefinition.Push(
                                field: currentPropertyPath,
                                value: propertyValueArray.GetValue(i));
                        }
                    }

                    else if (property.PropertyType.GetInterface(typeof(IDictionary<,>).Name) is not null)
                    {
                        var propertyValueDictionary = propertyValue as IDictionary;
                        var keysEnumerator = propertyValueDictionary?.Keys.GetEnumerator();
                        var valuesEnumerator = propertyValueDictionary?.Values.GetEnumerator();

                        if (keysEnumerator is not null && valuesEnumerator is not null)
                        {
                            while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext())
                            {
                                updateDefinition = updateDefinition.Set(
                                    field: $"{currentPropertyPath}.{keysEnumerator.Current}",
                                    value: valuesEnumerator.Current);
                            }
                        }
                    }

                    else if (property.PropertyType.GetInterface(typeof(IEnumerable<>).Name) is not null)
                    {
                        var propertyValueList = propertyValue as IEnumerable;
                        var propertyValueEnumerator = propertyValueList?.GetEnumerator();

                        if (propertyValueEnumerator is not null)
                        {
                            while (propertyValueEnumerator.MoveNext())
                            {
                                updateDefinition = updateDefinition.Push(
                                    field: currentPropertyPath,
                                    value: propertyValueEnumerator.Current);
                            }
                        }
                    }

                    else
                    {
                        updateDefinition = Builders<MongoDocument<TData>>
                            .Update
                            .Combine(updateDefinition, CreatePatchDefinition(propertyValue, updateDefinition, currentPropertyPath));
                    }
                }

                else
                {
                    updateDefinition = updateDefinition.Set(
                        field: currentPropertyPath,
                        value: propertyValue);
                }
            }

            return updateDefinition;
        }

        public async Task PruneDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            Expression<Func<MongoDocument<TData>, bool>> pruneFilterExpression =
                (x) => x.Id.Equals(document.Id) && x.UpdateVersion.Equals(document.UpdateVersion);

            var pruneDefinition = CreatePruneDefinition(
                data: document,
                updateDefinition: Builders<MongoDocument<TData>>.Update.Set(
                    field: f => f.Id,
                    value: document.Id));

            if (updateDocumentAuditingInformation)
            {
                pruneDefinition = _mongoDocumentAuditingService.UpdateDocumentAuditingInformation(pruneDefinition);
            }

            var pruneResult = session is not null ?
                await Collection.UpdateOneAsync(session, pruneFilterExpression, pruneDefinition) :
                await Collection.UpdateOneAsync(pruneFilterExpression, pruneDefinition);

            if (pruneResult.ModifiedCount is 0)
            {
                if (await DocumentExistsAsync(x => x.Id == document.Id))
                    throw new MongoConcurrencyException(document.Id);

                throw new DocumentNotFoundException(_collectionName, document.Id);
            }
        }

        public async Task PruneDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null, bool updateDocumentAuditingInformation = false)
        {
            var mongoExceptions = new List<MongoMultipleExceptionDetail>();

            foreach (var document in documents)
            {
                try
                {
                    await PruneDocumentAsync(document, session, updateDocumentAuditingInformation);
                }
                catch (Exception ex)
                {
                    mongoExceptions.Add(new MongoMultipleExceptionDetail(
                        collectionName: Collection.CollectionNamespace.CollectionName,
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        exception: ex));
                }
            }

            if (mongoExceptions?.Any() ?? false)
            {
                throw new MongoMultipleException(
                    message: "Multiple errors have occurred during the bulk prune operation.",
                    mongoExceptions: mongoExceptions);
            }
        }

        private UpdateDefinition<MongoDocument<TData>> CreatePruneDefinition<T>(T data, UpdateDefinition<MongoDocument<TData>> updateDefinition, string propertyPath = "")
        {
            foreach (var property in data?.GetType().GetProperties())
            {
                string currentPropertyPath = string.IsNullOrEmpty(propertyPath) ?
                    property.Name.ToCamelCase() :
                    $"{propertyPath}.{property.Name.ToCamelCase()}";

                var propertyValue = data?.GetType()?.GetProperty(property.Name)?.GetValue(data);

                if (propertyValue is null)
                    continue;

                if (currentPropertyPath == nameof(IIdentifiable.Id).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IDocument<TData>.CreateDateTime).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IDocument<TData>.CreateUser).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IPersistedDocument.UpdateDateTime).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IPersistedDocument.UpdateUser).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IPersistedDocument.UpdateVersion).ToCamelCase())
                    continue;

                if (currentPropertyPath == nameof(IDocument<TData>.ETag).ToCamelCase())
                    continue;

                if (property.PropertyType.IsClass && !property.PropertyType.IsPrimitive &&
                    property.PropertyType != typeof(object) && property.PropertyType != typeof(string))
                {
                    if (property.PropertyType.IsArray)
                    {
                        var propertyValueArray = propertyValue as Array;

                        for (int i = 0; i < propertyValueArray?.Length; i++)
                        {
                            updateDefinition = updateDefinition.Pull(
                                field: currentPropertyPath,
                                value: propertyValueArray.GetValue(i));
                        }
                    }

                    else if (property.PropertyType.GetInterface(typeof(IDictionary<,>).Name) is not null)
                    {
                        var propertyValueDictionary = propertyValue as IDictionary;
                        var keysEnumerator = propertyValueDictionary?.Keys.GetEnumerator();
                        var valuesEnumerator = propertyValueDictionary?.Values.GetEnumerator();

                        if (keysEnumerator is not null && valuesEnumerator is not null)
                        {
                            while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext())
                            {
                                updateDefinition = updateDefinition.Unset(
                                    field: $"{currentPropertyPath}.{keysEnumerator.Current}");
                            }
                        }
                    }

                    else if (property.PropertyType.GetInterface(typeof(IEnumerable<>).Name) is not null)
                    {
                        var propertyValueList = propertyValue as IEnumerable;
                        var propertyValueEnumerator = propertyValueList?.GetEnumerator();

                        if (propertyValueEnumerator is not null)
                        {
                            while (propertyValueEnumerator.MoveNext())
                            {
                                updateDefinition = updateDefinition.Pull(
                                    field: currentPropertyPath,
                                    value: propertyValueEnumerator.Current);
                            }
                        }
                    }

                    else
                    {
                        updateDefinition = Builders<MongoDocument<TData>>
                            .Update
                            .Combine(updateDefinition, CreatePruneDefinition(propertyValue, updateDefinition, currentPropertyPath));
                    }
                }

                else
                {
                    updateDefinition = updateDefinition.Unset(
                        field: currentPropertyPath);
                }
            }

            return updateDefinition;
        }

        public async Task DeleteDocumentAsync(Guid id, ulong updateVersion, IClientSessionHandle? session = null)
        {
            Expression<Func<MongoDocument<TData>, bool>> deleteFilterExpression = (x) => x.Id == id && x.UpdateVersion.Equals(updateVersion);

            var deleteResult = session != null ?
                await Collection.DeleteOneAsync(session, deleteFilterExpression) :
                await Collection.DeleteOneAsync(deleteFilterExpression);

            if (deleteResult.DeletedCount == 0)
            {
                if (await DocumentExistsAsync(x => x.Id == id))
                    throw new MongoConcurrencyException(id);

                throw new DocumentNotFoundException(_collectionName, id);
            }
        }

        public async Task DeleteDocumentAsync(MongoDocument<TData> document, IClientSessionHandle? session = null)
        {
            await DeleteDocumentAsync(document.Id, document.UpdateVersion, session);
        }

        public async Task DeleteDocumentsAsync(IEnumerable<MongoDocument<TData>> documents, IClientSessionHandle? session = null)
        {
            var mongoExceptions = new List<MongoMultipleExceptionDetail>();

            foreach (MongoDocument<TData> document in documents)
            {
                try
                {
                    await DeleteDocumentAsync(document, session);
                }
                catch (Exception exception)
                {
                    mongoExceptions.Add(new MongoMultipleExceptionDetail(
                        collectionName: Collection.CollectionNamespace.CollectionName,
                        id: document.Id,
                        updateVersion: document.UpdateVersion,
                        exception: exception));
                }
            }

            if (mongoExceptions?.Any() ?? false)
            {
                throw new MongoMultipleException(
                    message: "Multiple errors have occurred during the bulk delete operation.",
                    mongoExceptions: mongoExceptions);
            }
        }

        #endregion Methods
    }
}

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.CQRS.Data;
using Rested.Core.CQRS.Queries;
using Rested.Core.CQRS.Validation;
using Rested.Mongo.CQRS.Data;

namespace Rested.Mongo.CQRS.Queries
{
    public abstract class GetMongoDocumentQuery<TData> : GetDocumentQuery<TData, MongoDocument<TData>> where TData : IData
    {
        #region Ctor

        public GetMongoDocumentQuery(Guid id) : base(id)
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoDocumentQueryValidator<TData, TGetMongoDocumentQuery> : GetDocumentQueryValidator<TData, MongoDocument<TData>, TGetMongoDocumentQuery>
        where TData : IData
        where TGetMongoDocumentQuery : GetMongoDocumentQuery<TData>
    {
        #region Ctor

        public GetMongoDocumentQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoDocumentQueryHandler<TData, TGetMongoDocumentQuery> : GetDocumentQueryHandler<TData, MongoDocument<TData>, TGetMongoDocumentQuery>
        where TData : IData
        where TGetMongoDocumentQuery : GetMongoDocumentQuery<TData>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public GetMongoDocumentQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory)
        {
            _mongoContext = mongoContext;
        }

        #endregion Ctor

        #region Methods

        protected override void OnCheckDependencies()
        {
            base.OnCheckDependencies();

            if (_mongoContext is null)
                throw new NullReferenceException(
                    message: $"{nameof(IMongoContext)} was not injected.");
        }

        public override async Task<MongoDocument<TData>> Handle(TGetMongoDocumentQuery query, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: query,
                paramName: nameof(query));

            try
            {
                OnBeginHandle(query);

                var mongoDocument = await GetDocument(query.Id);

                OnHandleComplete(query, mongoDocument);

                return mongoDocument;
            }
            catch (MongoQueryException mongoQueryException)
            {
                _logger.LogError(mongoQueryException, $"{GetType().Name} Error");

                OnMongoQueryException(mongoQueryException);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Error");

                throw;
            }
        }

        protected override async Task<MongoDocument<TData>> GetDocument(Guid id)
        {
            return await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .GetDocumentAsync(id);
        }

        protected virtual void OnMongoQueryException(MongoQueryException mongoQueryException)
        {
            ValidationExceptionFactory.Throw(
                serviceErrorCode: ServiceErrorCodes.CommonErrorCodes.QueryError,
                messageArgs: mongoQueryException.Message);
        }

        #endregion Methods
    }
}

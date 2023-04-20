using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.CQRS.Data;
using Rested.Core.CQRS.Queries;
using Rested.Core.CQRS.Validation;
using Rested.Mongo.CQRS.Data;

namespace Rested.Mongo.CQRS.Queries
{
    public abstract class GetMongoDocumentsQuery<TData> :
        GetDocumentsQuery<TData, MongoDocument<TData>>
        where TData : IData
    {
        #region Ctor

        public GetMongoDocumentsQuery()
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoDocumentsQueryValidator<TData, TGetMongoDocumentsQuery> :
        GetDocumentsQueryValidator<TData, MongoDocument<TData>, TGetMongoDocumentsQuery>
        where TData : IData
        where TGetMongoDocumentsQuery : GetMongoDocumentsQuery<TData>
    {
        #region Ctor

        public GetMongoDocumentsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoDocumentsQueryHandler<TData, TGetMongoDocumentsQuery> :
        GetDocumentsQueryHandler<TData, MongoDocument<TData>, TGetMongoDocumentsQuery>
        where TData : IData
        where TGetMongoDocumentsQuery : GetMongoDocumentsQuery<TData>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public GetMongoDocumentsQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) : base(loggerFactory)
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

        public override async Task<List<MongoDocument<TData>>> Handle(TGetMongoDocumentsQuery query, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: query,
                paramName: nameof(query));

            try
            {
                OnBeginHandle(query);

                var mongoDocuments = await GetDocuments();

                OnHandleComplete(query, mongoDocuments);

                return mongoDocuments;
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

        protected override async Task<List<MongoDocument<TData>>> GetDocuments()
        {
            var result = await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .FindDocumentsAsync(mongoDocument => true);

            return result.ToList();
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

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Data;
using Rested.Core.Queries;
using Rested.Core.Validation;
using Rested.Mongo.Data;

namespace Rested.Mongo.Queries
{
    public abstract class SearchMongoDocumentsQuery<TData> :
        SearchDocumentsQuery<TData, MongoDocument<TData>>
        where TData : IData
    {
        #region Ctor

        public SearchMongoDocumentsQuery(SearchRequest searchRequest) : base(searchRequest)
        {

        }

        #endregion Ctor
    }

    public abstract class SearchMongoDocumentsQueryValidator<TData, TSearchMongoDocumentsQuery> :
        SearchDocumentsQueryValidator<TData, MongoDocument<TData>, TSearchMongoDocumentsQuery>
        where TData : IData
        where TSearchMongoDocumentsQuery : SearchMongoDocumentsQuery<TData>
    {
        #region Ctor

        public SearchMongoDocumentsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class SearchMongoDocumentsQueryHandler<TData, TSearchMongoDocumentsQuery> :
        SearchDocumentsQueryHandler<TData, MongoDocument<TData>, TSearchMongoDocumentsQuery>
        where TData : IData
        where TSearchMongoDocumentsQuery : SearchMongoDocumentsQuery<TData>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public SearchMongoDocumentsQueryHandler(
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

        public override async Task<SearchDocumentsResults<TData, MongoDocument<TData>>> Handle(TSearchMongoDocumentsQuery query, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: query,
                paramName: nameof(query));

            try
            {
                OnBeginHandle(query);

                var searchMongoDocumentsResults = await GetSearchResults(query);

                OnHandleComplete(query, searchMongoDocumentsResults);

                return searchMongoDocumentsResults;
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

        protected override async Task<SearchDocumentsResults<TData, MongoDocument<TData>>> GetSearchResults(TSearchMongoDocumentsQuery query)
        {
            return await SearchMongoQueryFactory.SearchMongoDocuments<TData>(
                mongoContext: _mongoContext,
                searchRequest: query.SearchRequest,
                implicitFieldFilters: GetImplicitFieldFilters());
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

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Data;
using Rested.Core.Queries;
using Rested.Core.Validation;
using Rested.Mongo.Data;

namespace Rested.Mongo.Queries
{
    public abstract class SearchMongoProjectionsQuery<TData, TProjection> :
        SearchProjectionsQuery<TData, TProjection>
        where TData : IData
        where TProjection : Projection
    {
        #region Ctor

        public SearchMongoProjectionsQuery(SearchRequest searchRequest) : base(searchRequest)
        {

        }

        #endregion Ctor
    }

    public abstract class SearchMongoProjectionsQueryValidator<TData, TProjection, TSearchMongoProjectionsQuery> :
        SearchProjectionsQueryValidator<TData, TProjection, TSearchMongoProjectionsQuery>
        where TData : IData
        where TProjection : Projection
        where TSearchMongoProjectionsQuery : SearchMongoProjectionsQuery<TData, TProjection>
    {
        #region Ctor

        public SearchMongoProjectionsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class SearchMongoProjectionsQueryHandler<TData, TProjection, TSearchMongoProjectionsQuery> :
        SearchProjectionsQueryHandler<TData, TProjection, TSearchMongoProjectionsQuery>
        where TData : IData
        where TProjection : Projection
        where TSearchMongoProjectionsQuery : SearchMongoProjectionsQuery<TData, TProjection>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public SearchMongoProjectionsQueryHandler(
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

        public override async Task<SearchProjectionsResults<TData, TProjection>> Handle(TSearchMongoProjectionsQuery query, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: query,
                paramName: nameof(query));

            try
            {
                OnBeginHandle(query);

                var searchMongoProjectionsResults = await GetSearchResults(query);

                OnHandleComplete(query, searchMongoProjectionsResults);

                return searchMongoProjectionsResults;
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

        protected override async Task<SearchProjectionsResults<TData, TProjection>> GetSearchResults(TSearchMongoProjectionsQuery query)
        {
            return await SearchMongoQueryFactory.SearchMongoProjections<TData, TProjection>(
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

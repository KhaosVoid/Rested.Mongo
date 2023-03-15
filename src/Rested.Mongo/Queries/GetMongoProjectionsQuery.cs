using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Data;
using Rested.Core.Queries;
using Rested.Core.Validation;
using Rested.Mongo.Data;

namespace Rested.Mongo.Queries
{
    public abstract class GetMongoProjectionsQuery<TData, TProjection> :
        GetProjectionsQuery<TData, TProjection>
        where TData : IData
        where TProjection : Projection
    {
        #region Ctor

        public GetMongoProjectionsQuery() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoProjectionsQueryValidator<TData, TProjection, TGetMongoProjectionsQuery> :
        GetProjectionsQueryValidator<TData, TProjection, TGetMongoProjectionsQuery>
        where TData : IData
        where TProjection : Projection
        where TGetMongoProjectionsQuery : GetMongoProjectionsQuery<TData, TProjection>
    {
        #region Ctor

        public GetMongoProjectionsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoProjectionsQueryHandler<TData, TProjection, TGetMongoProjectionsQuery> :
        GetProjectionsQueryHandler<TData, TProjection, TGetMongoProjectionsQuery>
        where TData : IData
        where TProjection : Projection
        where TGetMongoProjectionsQuery : GetMongoProjectionsQuery<TData, TProjection>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public GetMongoProjectionsQueryHandler(
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

        public override async Task<List<TProjection>> Handle(TGetMongoProjectionsQuery query, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: query,
                paramName: nameof(query));

            try
            {
                OnBeginHandle(query);

                var projections = await GetProjections();

                OnHandleComplete(query, projections);

                return projections;
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

        protected override async Task<List<TProjection>> GetProjections()
        {
            var projectionDefinition = Builders<MongoDocument<TData>>
                .Projection
                .Expression(Projection.GetProjectionExpression<TProjection, MongoDocument<TData>>());

            var result = await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .FindProjectionsAsync(
                    filterDefinition: Builders<MongoDocument<TData>>.Filter.Empty,
                    projectionDefinition: projectionDefinition);

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

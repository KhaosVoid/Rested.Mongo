using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Data;
using Rested.Core.MediatR.Queries;
using Rested.Core.MediatR.Validation;
using Rested.Mongo.Data;

namespace Rested.Mongo.MediatR.Queries
{
    public abstract class GetMongoProjectionQuery<TData, TProjection> :
        GetProjectionQuery<TData, TProjection>
        where TData : IData
        where TProjection : Projection
    {
        #region Ctor

        public GetMongoProjectionQuery(Guid id) : base(id)
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoProjectionQueryValidator<TData, TProjection, TGetMongoProjectionQuery> :
        GetProjectionQueryValidator<TData, TProjection, TGetMongoProjectionQuery>
        where TData : IData
        where TProjection : Projection
        where TGetMongoProjectionQuery : GetProjectionQuery<TData, TProjection>
    {
        #region Ctor

        public GetMongoProjectionQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class GetMongoProjectionQueryHandler<TData, TProjection, TGetMongoProjectionQuery> :
        GetProjectionQueryHandler<TData, TProjection, TGetMongoProjectionQuery>
        where TData : IData
        where TProjection : Projection
        where TGetMongoProjectionQuery : GetMongoProjectionQuery<TData, TProjection>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public GetMongoProjectionQueryHandler(
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

        public override async Task<TProjection> Handle(TGetMongoProjectionQuery query, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: query,
                paramName: nameof(query));

            try
            {
                OnBeginHandle(query);

                var projection = await GetProjection(query.Id);

                OnHandleComplete(query, projection);

                return projection;
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

        protected override async Task<TProjection> GetProjection(Guid id)
        {
            var projectionDefinition = Builders<MongoDocument<TData>>
                .Projection
                .Expression(Projection.GetProjectionExpression<TProjection, MongoDocument<TData>>());

            return await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .GetProjectionAsync(
                    predicate: mongoDocument => mongoDocument.Id == id,
                    projectionDefinition: projectionDefinition);
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

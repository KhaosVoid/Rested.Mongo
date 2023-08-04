using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rested.Core.Data;
using Rested.Core.Server.Mvc;
using Rested.Mongo.MediatR.Queries;

namespace Rested.Mongo.Server.Mvc
{
    public abstract class MongoProjectionController<TData, TProjection> :
        ProjectionController<TData, TProjection>
        where TData : IData
        where TProjection : Projection
    {
        #region Ctor

        public MongoProjectionController(
            IMediator mediator,
            IHttpContextAccessor httpContext,
            ILoggerFactory loggerFactory) :
                base(mediator, httpContext, loggerFactory)
        {

        }

        #endregion Ctor

        #region Methods

        protected abstract GetMongoProjectionQuery<TData, TProjection> CreateGetMongoProjectionQuery(Guid id);
        protected abstract GetMongoProjectionsQuery<TData, TProjection> CreateGetMongoProjectionsQuery();
        protected abstract SearchMongoProjectionsQuery<TData, TProjection> CreateSearchMongoProjectionsQuery(SearchRequest searchRequest);

        public override async Task<ActionResult<TProjection>> GetProjection([FromRoute] Guid id)
        {
            try
            {
                var query = CreateGetMongoProjectionQuery(id);
                var result = await _mediator.Send(query);

                if (result is null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to execute {GetType().Name}.{nameof(GetProjection)}().");

                throw;
            }
        }

        public override async Task<ActionResult<List<TProjection>>> GetProjections()
        {
            try
            {
                var query = CreateGetMongoProjectionsQuery();
                var result = await _mediator.Send(query);

                if (result is null || result.Count is 0)
                    return NoContent();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to execute {GetType().Name}.{nameof(GetProjections)}().");

                throw;
            }
        }

        public override async Task<ActionResult<SearchProjectionsResults<TProjection>>> SearchProjections([FromBody] SearchRequest searchRequest)
        {
            try
            {
                var query = CreateSearchMongoProjectionsQuery(searchRequest);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to execute {GetType().Name}.{nameof(SearchProjections)}().");
                throw;
            }
        }

        #endregion Methods
    }
}

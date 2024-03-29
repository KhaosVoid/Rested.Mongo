﻿using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rested.Core.Data;
using Rested.Core.MediatR.Commands;
using Rested.Core.Server.Http;
using Rested.Core.Server.Mvc;
using Rested.Mongo.MediatR.Commands;
using Rested.Mongo.MediatR.Queries;

namespace Rested.Mongo.Server.Mvc
{
    public abstract class MongoDocumentController<TData> :
        DocumentController<TData>
        where TData : IData
    {
        #region Ctor

        public MongoDocumentController(
            IMediator mediator,
            IHttpContextAccessor httpContext,
            ILoggerFactory loggerFactory) :
                base(mediator, httpContext, loggerFactory)
        {

        }

        #endregion Ctor

        #region Methods

        protected abstract GetMongoDocumentQuery<TData> CreateGetMongoDocumentQuery(Guid id);
        protected abstract GetMongoDocumentsQuery<TData> CreateGetMongoDocumentsQuery();
        protected abstract SearchMongoDocumentsQuery<TData> CreateSearchMongoDocumentsQuery(SearchRequest searchRequest);
        protected abstract MongoDtoCommand<TData> CreateDtoCommand(Dto<TData> dto, CommandActions action);
        protected abstract MultiMongoDtoCommand<TData> CreateMultiDtoCommand(List<Dto<TData>> dtos, CommandActions action);

        public override async Task<ActionResult<IDocument<TData>>> GetDocument([FromRoute] Guid id)
        {
            try
            {
                var query = CreateGetMongoDocumentQuery(id);
                var result = await _mediator.Send(query);

                if (result is null)
                    return NotFound();

                _httpContext.AddETagResponseHeader(result.ETag);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to execute {GetType().Name}.{nameof(GetDocument)}().");

                throw;
            }
        }

        public override async Task<ActionResult<List<IDocument<TData>>>> GetDocuments()
        {
            try
            {
                var query = CreateGetMongoDocumentsQuery();
                var result = await _mediator.Send(query);

                if (result is null || result.Count is 0)
                    return NoContent();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to execute {GetType().Name}.{nameof(GetDocuments)}().");

                throw;
            }
        }

        public override async Task<ActionResult<SearchDocumentsResults<TData, IDocument<TData>>>> SearchDocuments([FromBody] SearchRequest searchRequest)
        {
            try
            {
                var query = CreateSearchMongoDocumentsQuery(searchRequest);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to execute {GetType().Name}.{nameof(SearchDocuments)}().");
                throw;
            }
        }

        public override async Task<ActionResult<IDocument<TData>>> InsertDocument([FromBody] TData data)
        {
            try
            {
                var dto = new Dto<TData>()
                {
                    Data = data
                };

                var command = CreateDtoCommand(dto, CommandActions.Insert);
                var result = await _mediator.Send(command);

                if (result is not null)
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result.UpdateVersion));

                return Created($"{_httpContext.HttpContext.Request.Path}/{result.Id}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to insert {typeof(TData).Name}.");

                throw;
            }
        }

        public override async Task<ActionResult<List<IDocument<TData>>>> InsertMultipleDocuments([FromBody] List<TData> datas)
        {
            try
            {
                var dtos = datas?
                    .Select(data => Dto<TData>.FromData(data))
                    .ToList();

                var command = CreateMultiDtoCommand(dtos, CommandActions.Insert);
                var result = await _mediator.Send(command);

                if (result is not null && result.Count > 0)
                {
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result[0].UpdateVersion));
                    return Created($"{_httpContext.HttpContext.Request.Path}/{result[0].Id}", result);
                }

                throw new ApplicationException("No data returned multiple dto insert handler.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to insert multiple {typeof(TData).Name}s.");

                throw;
            }
        }

        public override async Task<ActionResult<IDocument<TData>>> UpdateDocument(
            [FromRoute] Guid id,
            [FromHeader] IfMatchByteArray etag,
            [FromBody] TData data)
        {
            try
            {
                var dto = new Dto<TData>()
                {
                    Id = id,
                    ETag = etag.Tag,
                    Data = data
                };

                var command = CreateDtoCommand(dto, CommandActions.Update);
                var result = await _mediator.Send(command);

                if (result is not null)
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result.UpdateVersion));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update {typeof(TData).Name}.");

                throw;
            }
        }

        public override async Task<ActionResult<List<IDocument<TData>>>> UpdateMultipleDocuments([FromBody] List<Dto<TData>> dtos)
        {
            try
            {
                var command = CreateMultiDtoCommand(dtos, CommandActions.Update);
                var result = await _mediator.Send(command);

                if (result is not null && result.Count > 0)
                {
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result[0].UpdateVersion));
                    return Ok(result);
                }
                throw new ApplicationException("No data returned multiple dto update handler.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update multiple {typeof(TData).Name}s.");

                throw;
            }
        }

        public override async Task<ActionResult<IDocument<TData>>> PatchDocument(
            [FromRoute] Guid id,
            [FromHeader] IfMatchByteArray etag,
            [FromBody] TData data)
        {
            try
            {
                var dto = new Dto<TData>()
                {
                    Id = id,
                    ETag = etag.Tag,
                    Data = data
                };

                var command = CreateDtoCommand(dto, CommandActions.Patch);
                var result = await _mediator.Send(command);

                if (result is not null)
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result.UpdateVersion));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to patch {typeof(TData).Name}.");

                throw;
            }
        }

        public override async Task<ActionResult<List<IDocument<TData>>>> PatchMultipleDocuments([FromBody] List<Dto<TData>> dtos)
        {
            try
            {
                var command = CreateMultiDtoCommand(dtos, CommandActions.Patch);
                var result = await _mediator.Send(command);

                if (result is not null && result.Count > 0)
                {
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result[0].UpdateVersion));
                    return Ok(result);
                }

                throw new ApplicationException("No data returned multiple dto patch handler.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to patch multiple {typeof(TData).Name}s.");

                throw;
            }
        }

        public override async Task<ActionResult<IDocument<TData>>> PruneDocument(
            [FromRoute] Guid id,
            [FromHeader] IfMatchByteArray etag,
            [FromBody] TData data)
        {
            try
            {
                var dto = new Dto<TData>()
                {
                    Id = id,
                    ETag = etag.Tag,
                    Data = data
                };

                var command = CreateDtoCommand(dto, CommandActions.Prune);
                var result = await _mediator.Send(command);

                if (result is not null)
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result.UpdateVersion));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to prune {typeof(TData).Name}.");

                throw;
            }
        }

        public override async Task<ActionResult<List<IDocument<TData>>>> PruneMultipleDocuments([FromBody] List<Dto<TData>> dtos)
        {
            try
            {
                var command = CreateMultiDtoCommand(dtos, CommandActions.Prune);
                var result = await _mediator.Send(command);

                if (result is not null && result.Count > 0)
                {
                    _httpContext.AddETagResponseHeader(BitConverter.GetBytes(result[0].UpdateVersion));
                    return Ok(result);
                }

                throw new ApplicationException("No data returned multiple dto patch handler.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to prune multiple {typeof(TData).Name}s.");

                throw;
            }
        }

        public override async Task<NoContentResult> DeleteDocument(
            [FromRoute] Guid id,
            [FromHeader] IfMatchByteArray etag)
        {
            try
            {
                var dto = new Dto<TData>()
                {
                    Id = id,
                    ETag = etag.Tag
                };

                var command = CreateDtoCommand(dto, CommandActions.Delete);
                var result = await _mediator.Send(command);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete {typeof(TData).Name}.");

                throw;
            }
        }

        public override async Task<NoContentResult> DeleteMultipleDocuments([FromBody] List<BaseDto> baseDtos)
        {
            try
            {
                var dtos = Dto<TData>.ToList(baseDtos);
                var command = CreateMultiDtoCommand(dtos, CommandActions.Delete);
                var result = await _mediator.Send(command);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete multiple {typeof(TData).Name}s.");

                throw;
            }
        }

        #endregion Methods
    }
}

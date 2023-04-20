using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.CQRS.Commands;
using Rested.Core.CQRS.Data;
using Rested.Core.CQRS.Validation;
using Rested.Mongo.CQRS.Data;
using Rested.Mongo.CQRS.Validation;

namespace Rested.Mongo.CQRS.Commands
{
    public abstract class MongoCommand<TData> : DocumentCommand<TData, MongoDocument<TData>> where TData : IData
    {
        #region Ctor

        public MongoCommand(CommandActions action) : base(action)
        {

        }

        #endregion Ctor
    }

    public abstract class MongoCommandValidator<TData, TMongoCommand> : DocumentCommandValidator<TData, MongoDocument<TData>, TMongoCommand>
        where TData : IData
        where TMongoCommand : MongoCommand<TData>
    {
        #region Ctor

        public MongoCommandValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class MongoCommandHandler<TData, TMongoCommand> : DocumentCommandHandler<TData, MongoDocument<TData>, TMongoCommand>
        where TData : IData
        where TMongoCommand : MongoCommand<TData>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public MongoCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory)
        {
            _mongoContext = mongoContext;
        }

        #endregion Ctor

        #region Methods

        public override async Task<MongoDocument<TData>> Handle(TMongoCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                argument: command,
                paramName: nameof(command));

            using var session = await _mongoContext.SessionFactory.CreateAsync();
            var mongoDocument = CreateDocumentFromCommand(command);

            try
            {
                OnBeginHandle(command, mongoDocument);

                session.StartTransaction();

                switch (command.Action)
                {
                    case CommandActions.Insert:
                        OnSetPrecalculatedProperties(command, mongoDocument);
                        await OnInsertDocument(session, command, mongoDocument);
                        break;

                    case CommandActions.Update:
                        OnSetPrecalculatedProperties(command, mongoDocument);
                        await OnUpdateDocument(session, command, mongoDocument);
                        break;

                    case CommandActions.Patch:
                        OnSetPrecalculatedProperties(command, mongoDocument);
                        await OnPatchDocument(session, command, mongoDocument);
                        break;

                    case CommandActions.Prune:
                        await OnPruneDocument(session, command, mongoDocument);
                        break;

                    case CommandActions.Delete:
                        await OnDeleteDocument(session, command, mongoDocument);
                        break;
                }

                await session.CommitTransactionAsync(cancellationToken);

                mongoDocument = await GetDocument(mongoDocument.Id);

                OnHandleComplete(command, mongoDocument);

                return mongoDocument;
            }
            catch (MongoWriteException mongoWriteException)
            {
                TryAbortTransactionAsync(session, cancellationToken);

                _logger.LogError(mongoWriteException, $"{GetType().Name} Error");

                OnMongoWriteException(mongoWriteException);

                throw;
            }
            catch (Exception ex)
            {
                TryAbortTransactionAsync(session, cancellationToken);

                _logger.LogError(ex, $"{GetType().Name} Error");

                throw;
            }
        }

        protected virtual async Task OnInsertDocument(IClientSessionHandle session, TMongoCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .InsertDocumentAsync(
                    document: mongoDocument,
                    session: session,
                    setDocumentAuditingInformation: true);
        }

        protected virtual async Task OnUpdateDocument(IClientSessionHandle session, TMongoCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .UpdateDocumentDataAsync(
                    document: mongoDocument,
                    session: session,
                    updateDocumentAuditingInformation: true);
        }

        protected virtual async Task OnPatchDocument(IClientSessionHandle session, TMongoCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .PatchDocumentAsync(
                    document: mongoDocument,
                    session: session,
                    updateDocumentAuditingInformation: true);
        }

        protected virtual async Task OnPruneDocument(IClientSessionHandle session, TMongoCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .PruneDocumentAsync(
                    document: mongoDocument,
                    session: session,
                    updateDocumentAuditingInformation: true);
        }

        protected virtual async Task OnDeleteDocument(IClientSessionHandle session, TMongoCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .DeleteDocumentAsync(
                    id: mongoDocument.Id,
                    updateVersion: mongoDocument.UpdateVersion,
                    session: session);
        }

        protected async void TryAbortTransactionAsync(IClientSessionHandle session, CancellationToken cancellationToken)
        {
            try
            {
                await session.AbortTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while attempting to abort the current transaction. Command: {GetType().Name}");
            }
        }

        protected virtual void OnMongoWriteException(MongoWriteException mongoWriteException)
        {
            MongoValidationExceptionFactory.ThrowIfMongoIndexViolation(mongoWriteException, ServiceErrorCodes);

            ValidationExceptionFactory.Throw(
                serviceErrorCode: ServiceErrorCodes.CommonErrorCodes.DatabaseError,
                messageArgs: mongoWriteException.WriteError.Message);
        }

        protected override async Task<MongoDocument<TData>> GetDocument(Guid id)
        {
            return await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .GetDocumentAsync(id);
        }

        #endregion Methods
    }
}

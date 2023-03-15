using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Commands;
using Rested.Core.Data;
using Rested.Core.Validation;
using Rested.Mongo.Data;
using Rested.Mongo.Validation;

namespace Rested.Mongo.Commands
{
    public abstract class MultiMongoCommand<TData> : MultiDocumentCommand<TData, MongoDocument<TData>> where TData : IData
    {
        #region Ctor

        public MultiMongoCommand(CommandActions action) : base(action)
        {

        }

        #endregion Ctor
    }

    public abstract class MultiMongoCommandValidator<TData, TMultiMongoCommand> : MultiDocumentCommandValidator<TData, MongoDocument<TData>, TMultiMongoCommand>
        where TData : IData
        where TMultiMongoCommand : MultiMongoCommand<TData>
    {
        #region Ctor

        public MultiMongoCommandValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class MultiMongoCommandHandler<TData, TMultiMongoCommand> : MultiDocumentCommandHandler<TData, MongoDocument<TData>, TMultiMongoCommand>
        where TData : IData
        where TMultiMongoCommand : MultiMongoCommand<TData>
    {
        #region Members

        protected readonly IMongoContext _mongoContext;

        #endregion Members

        #region Ctor

        public MultiMongoCommandHandler(
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

        public override async Task<List<MongoDocument<TData>>> Handle(TMultiMongoCommand command, CancellationToken cancellationToken)
        {
            CheckDependencies();

            ArgumentNullException.ThrowIfNull(
                argument: command,
                paramName: nameof(command));

            using var session = await _mongoContext.SessionFactory.CreateAsync();
            var mongoDocuments = CreateDocumentListFromCommand(command);

            try
            {
                OnBeginHandle(command, mongoDocuments);

                session.StartTransaction();

                switch (command.Action)
                {
                    case CommandActions.Insert:
                        await OnInsertDocuments(session, command, mongoDocuments);
                        break;

                    case CommandActions.Update:
                        await OnUpdateDocuments(session, command, mongoDocuments);
                        break;

                    case CommandActions.Patch:
                        await OnPatchDocuments(session, command, mongoDocuments);
                        break;

                    case CommandActions.Delete:
                        await OnDeleteDocuements(session, command, mongoDocuments);
                        break;
                }

                await session.CommitTransactionAsync(cancellationToken);

                mongoDocuments = await GetDocuments(mongoDocuments.Select(mongoDocument => mongoDocument.Id).ToArray());

                OnHandleComplete(command, mongoDocuments);

                return mongoDocuments;
            }
            catch (MongoMultipleException mongoMultipleException)
            {
                TryAbortTransactionAsync(session, cancellationToken);

                _logger.LogError(mongoMultipleException, $"{GetType().Name} Error");

                OnMongoMultipleException(mongoMultipleException);

                throw;
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

        protected virtual async Task OnInsertDocuments(IClientSessionHandle session, TMultiMongoCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .InsertDocumentsAsync(
                    documents: mongoDocuments,
                    session: session,
                    setDocumentAuditingInformation: true);
        }

        protected virtual async Task OnUpdateDocuments(IClientSessionHandle session, TMultiMongoCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .UpdateDocumentsDataAsync(
                    documents: mongoDocuments,
                    session: session,
                    updateDocumentAuditingInformation: true);
        }

        protected virtual Task OnPatchDocuments(IClientSessionHandle session, TMultiMongoCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task OnDeleteDocuements(IClientSessionHandle session, TMultiMongoCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .DeleteDocumentsAsync(
                    documents: mongoDocuments,
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

        protected virtual void OnMongoMultipleException(MongoMultipleException mongoMultipleException) =>
            MongoValidationExceptionFactory.Throw(mongoMultipleException, ServiceErrorCodes);

        protected virtual void OnMongoWriteException(MongoWriteException mongoWriteException)
        {
            MongoValidationExceptionFactory.ThrowIfMongoIndexViolation(mongoWriteException, ServiceErrorCodes);

            ValidationExceptionFactory.Throw(
                serviceErrorCode: ServiceErrorCodes.CommonErrorCodes.DatabaseError,
                messageArgs: mongoWriteException.WriteError.Message);
        }

        protected override async Task<List<MongoDocument<TData>>> GetDocuments(Guid[] ids)
        {
            var filter = Builders<MongoDocument<TData>>
             .Filter
             .In(
                field: document => document.Id,
                values: ids);

            var result = await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .FindDocumentsAsync(filter);

            return result?.ToList();
        }

        #endregion Methods
    }
}

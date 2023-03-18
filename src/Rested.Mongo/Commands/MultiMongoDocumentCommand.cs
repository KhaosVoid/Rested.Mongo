using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Commands;
using Rested.Core.Data;
using Rested.Mongo.Data;

namespace Rested.Mongo.Commands
{
    public abstract class MultiMongoDocumentCommand<TData> : MultiMongoCommand<TData> where TData : IData
    {
        #region Properties

        public List<MongoDocument<TData>> Documents { get; }

        #endregion Properties

        #region Ctor

        public MultiMongoDocumentCommand(List<MongoDocument<TData>> documents, CommandActions action) : base(action)
        {
            Documents = documents;
        }

        #endregion Ctor
    }

    public abstract class MultiMongoDocumentCommandValidator<TData, TMultiMongoDocumentCommand> : MultiMongoCommandValidator<TData, TMultiMongoDocumentCommand>
        where TData : IData
        where TMultiMongoDocumentCommand : MultiMongoDocumentCommand<TData>
    {
        #region Ctor

        public MultiMongoDocumentCommandValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class MultiMongoDocumentCommandHandler<TData, TMultiMongoDocumentCommand> : MultiMongoCommandHandler<TData, TMultiMongoDocumentCommand>
        where TData : IData
        where TMultiMongoDocumentCommand : MultiMongoDocumentCommand<TData>
    {
        #region Ctor

        public MultiMongoDocumentCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor

        #region Methods

        protected override List<MongoDocument<TData>> CreateDocumentListFromCommand(TMultiMongoDocumentCommand command) => command.Documents;

        protected override async Task OnInsertDocuments(IClientSessionHandle session, TMultiMongoDocumentCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .InsertDocumentsAsync(
                    documents: mongoDocuments,
                    session: session);
        }

        protected override async Task OnUpdateDocuments(IClientSessionHandle session, TMultiMongoDocumentCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .UpdateDocumentsAsync(
                    documents: mongoDocuments,
                    session: session);
        }

        protected override async Task OnPatchDocuments(IClientSessionHandle session, TMultiMongoDocumentCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .PatchDocumentsAsync(
                    documents: mongoDocuments,
                    session: session);
        }

        protected override async Task OnDeleteDocuements(IClientSessionHandle session, TMultiMongoDocumentCommand command, List<MongoDocument<TData>> mongoDocuments)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .DeleteDocumentsAsync(
                    documents: mongoDocuments,
                    session: session);
        }

        #endregion Methods
    }
}

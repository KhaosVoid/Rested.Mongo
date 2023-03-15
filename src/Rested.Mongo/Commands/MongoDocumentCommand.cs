using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Rested.Core.Commands;
using Rested.Core.Data;
using Rested.Mongo.Data;

namespace Rested.Mongo.Commands
{
    public abstract class MongoDocumentCommand<TData> : MongoCommand<TData> where TData : IData
    {
        #region Properties

        public MongoDocument<TData> Document { get; }

        #endregion Properties

        #region Ctor

        public MongoDocumentCommand(MongoDocument<TData> document, CommandActions action) : base(action)
        {
            Document = document;
        }

        #endregion Ctor
    }

    public abstract class MongoDocumentCommandValidator<TData, TMongoDocumentCommand> : MongoCommandValidator<TData, TMongoDocumentCommand>
        where TData : IData
        where TMongoDocumentCommand : MongoDocumentCommand<TData>
    {
        #region Ctor

        public MongoDocumentCommandValidator() : base()
        {

        }

        #endregion Ctor
    }

    public abstract class MongoDocumentCommandHandler<TData, TMongoDocumentCommand> : MongoCommandHandler<TData, TMongoDocumentCommand>
        where TData : IData
        where TMongoDocumentCommand : MongoDocumentCommand<TData>
    {
        #region Ctor

        public MongoDocumentCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor

        #region Methods

        protected override MongoDocument<TData> CreateDocumentFromCommand(TMongoDocumentCommand command) => command.Document;

        protected override async Task OnInsertDocument(IClientSessionHandle session, TMongoDocumentCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .InsertDocumentAsync(
                    document: mongoDocument,
                    session: session);
        }

        protected override async Task OnUpdateDocument(IClientSessionHandle session, TMongoDocumentCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .UpdateDocumentAsync(
                    document: mongoDocument,
                    session: session);
        }

        protected override async Task OnDeleteDocument(IClientSessionHandle session, TMongoDocumentCommand command, MongoDocument<TData> mongoDocument)
        {
            await _mongoContext
                .RepositoryFactory
                .Create<TData>()
                .DeleteDocumentAsync(
                    document: mongoDocument,
                    session: session);
        }

        #endregion Methods
    }
}

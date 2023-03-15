using Rested.Core.Commands;
using Rested.Core.Data;
using Rested.Mongo.Commands;

namespace Rested.Mongo.MSTest.Commands
{
    public abstract class MongoDocumentCommandTest<TData, TMongoDocumentCommand, TMongoDocumentCommandValidator, TMongoDocumentCommandHandler> :
        MongoCommandTest<TData, TMongoDocumentCommand, TMongoDocumentCommandValidator, TMongoDocumentCommandHandler>
        where TData : IData
        where TMongoDocumentCommand : MongoDocumentCommand<TData>
        where TMongoDocumentCommandValidator : MongoDocumentCommandValidator<TData, TMongoDocumentCommand>
        where TMongoDocumentCommandHandler : MongoDocumentCommandHandler<TData, TMongoDocumentCommand>
    {
        #region Initialization

        protected override void OnInitializeTestDocument()
        {
            TestDocument = CreateDocument(data: InitializeTestData());
        }

        #endregion Initialization

        #region Methods

        protected override TMongoDocumentCommand CreateDocumentCommand(CommandActions action)
        {
            return (TMongoDocumentCommand)Activator.CreateInstance(
                type: typeof(TMongoDocumentCommand),
                args: new object[] { TestDocument, action });
        }

        #endregion Methods
    }
}

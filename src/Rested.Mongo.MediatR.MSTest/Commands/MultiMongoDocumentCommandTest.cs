﻿using Rested.Core.Data;
using Rested.Core.MediatR.Commands;
using Rested.Mongo.MediatR.Commands;

namespace Rested.Mongo.MediatR.MSTest.Commands
{
    public abstract class MultiMongoDocumentCommandTest<TData, TMultiMongoDocumentCommand, TMultiMongoDocumentCommandValidator, TMultiMongoDocumentCommandHandler> :
        MultiMongoCommandTest<TData, TMultiMongoDocumentCommand, TMultiMongoDocumentCommandValidator, TMultiMongoDocumentCommandHandler>
        where TData : IData
        where TMultiMongoDocumentCommand : MultiMongoDocumentCommand<TData>
        where TMultiMongoDocumentCommandValidator : MultiMongoDocumentCommandValidator<TData, TMultiMongoDocumentCommand>
        where TMultiMongoDocumentCommandHandler : MultiMongoDocumentCommandHandler<TData, TMultiMongoDocumentCommand>
    {
        #region Initialization

        protected override void OnInitializeTestDocuments()
        {
            TestDocuments = InitializeTestData().Select(CreateDocument).ToList();
        }

        #endregion Initialization

        #region Methods

        protected override TMultiMongoDocumentCommand CreateCommand(CommandActions action)
        {
            return (TMultiMongoDocumentCommand)Activator.CreateInstance(
                type: typeof(TMultiMongoDocumentCommand),
                args: new object[] { TestDocuments, action });
        }

        #endregion Methods
    }
}

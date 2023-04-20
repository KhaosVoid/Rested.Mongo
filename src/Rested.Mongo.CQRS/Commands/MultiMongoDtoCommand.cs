using FluentValidation;
using Microsoft.Extensions.Logging;
using Rested.Core.CQRS.Commands;
using Rested.Core.CQRS.Commands.Validation;
using Rested.Core.CQRS.Data;
using Rested.Mongo.CQRS.Data;

namespace Rested.Mongo.CQRS.Commands
{
    public abstract class MultiMongoDtoCommand<TData> : MultiMongoCommand<TData> where TData : IData
    {
        #region Properties

        public List<Dto<TData>> Dtos { get; }

        #endregion Properties

        #region Ctor

        public MultiMongoDtoCommand(List<Dto<TData>> dtos, CommandActions action) : base(action)
        {
            Dtos = dtos;
        }

        #endregion Ctor
    }

    public abstract class MultiMongoDtoCommandValidator<TData, TMultiMongoDtoCommand> : MultiMongoCommandValidator<TData, TMultiMongoDtoCommand>
        where TData : IData
        where TMultiMongoDtoCommand : MultiMongoDtoCommand<TData>
    {
        #region Ctor

        public MultiMongoDtoCommandValidator() : base()
        {
            var collectionMustNotBeEmptyErrorCode = ServiceErrorCodes.CommonErrorCodes.CollectionMustNotBeEmpty;

            RuleFor(m => m.Dtos)
                .NotEmpty()
                .WithMessage(collectionMustNotBeEmptyErrorCode.Message)
                .WithErrorCode(collectionMustNotBeEmptyErrorCode.ExtendedStatusCode);

            RuleForEach(command => command.Dtos)
                .SetValidator(command => new DtoValidator<TData>(command.Action, ServiceErrorCodes));
        }

        #endregion Ctor
    }

    public abstract class MultiMongoDtoCommandHandler<TData, TMultiMongoDtoCommand> : MultiMongoCommandHandler<TData, TMultiMongoDtoCommand>
        where TData : IData
        where TMultiMongoDtoCommand : MultiMongoDtoCommand<TData>
    {
        #region Ctor

        public MultiMongoDtoCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor

        #region Methods

        protected override List<MongoDocument<TData>> CreateDocumentListFromCommand(TMultiMongoDtoCommand command) =>
            command.Dtos.Select(MongoDocument<TData>.FromDto).ToList();

        #endregion Methods
    }
}

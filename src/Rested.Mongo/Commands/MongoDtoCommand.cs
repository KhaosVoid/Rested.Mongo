using Microsoft.Extensions.Logging;
using Rested.Core.Commands;
using Rested.Core.Commands.Validation;
using Rested.Core.Data;
using Rested.Mongo.Data;

namespace Rested.Mongo.Commands
{
    public abstract class MongoDtoCommand<TData> : MongoCommand<TData> where TData : IData
    {
        #region Properties

        public Dto<TData> Dto { get; }

        #endregion Properties

        #region Ctor

        public MongoDtoCommand(Dto<TData> dto, CommandActions action) : base(action)
        {
            Dto = dto;
        }

        #endregion Ctor
    }

    public abstract class MongoDtoCommandValidator<TData, TMongoDtoCommand> : MongoCommandValidator<TData, TMongoDtoCommand>
        where TData : IData
        where TMongoDtoCommand : MongoDtoCommand<TData>
    {
        #region Ctor

        public MongoDtoCommandValidator() : base()
        {
            RuleFor(command => command.Dto)
                .SetValidator(command => new DtoValidator<TData>(command.Action, ServiceErrorCodes));
        }

        #endregion Ctor
    }

    public abstract class MongoDtoCommandHandler<TData, TMongoDtoCommand> : MongoCommandHandler<TData, TMongoDtoCommand>
        where TData : IData
        where TMongoDtoCommand : MongoDtoCommand<TData>
    {
        #region Ctor

        public MongoDtoCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor

        #region Methods

        protected override MongoDocument<TData> CreateDocumentFromCommand(TMongoDtoCommand command) => MongoDocument<TData>.FromDto(command.Dto);

        #endregion Methods
    }
}

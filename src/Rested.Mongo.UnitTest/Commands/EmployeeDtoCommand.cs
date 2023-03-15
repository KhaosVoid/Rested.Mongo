using FluentValidation;
using Microsoft.Extensions.Logging;
using Rested.Core.Commands;
using Rested.Core.Data;
using Rested.Core.Validation;
using Rested.Mongo.Commands;
using Rested.Mongo.Data;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Commands
{
    public class EmployeeDtoCommand : MongoDtoCommand<Employee>
    {
        #region Ctor

        public EmployeeDtoCommand(
            Dto<Employee> dto,
            CommandActions action) :
                base(dto, action)
        {

        }

        #endregion Ctor
    }

    public class EmployeeDtoCommandValidator : MongoDtoCommandValidator<Employee, EmployeeDtoCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeDtoCommandValidator() : base()
        {
            When(command => command.Dto.Data != null, () =>
            {
                RuleFor(command => command.Dto.Data.FirstName)
                    .NotEmpty()
                    .WithServiceErrorCode(ServiceErrorCodes.FirstNameIsRequired);

                RuleFor(command => command.Dto.Data.LastName)
                    .NotEmpty()
                    .WithServiceErrorCode(ServiceErrorCodes.LastNameIsRequired);
            });
        }

        #endregion Ctor
    }

    public class EmployeeDtoCommandHandler : MongoDtoCommandHandler<Employee, EmployeeDtoCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeDtoCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor

        #region Methods

        protected override void OnBeginHandle(EmployeeDtoCommand command, MongoDocument<Employee> document)
        {
            document.Data.FullName = $"{document.Data.FirstName} {document.Data.LastName}";

            base.OnBeginHandle(command, document);
        }

        #endregion Methods
    }
}

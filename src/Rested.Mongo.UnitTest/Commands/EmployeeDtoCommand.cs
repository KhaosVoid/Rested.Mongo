using FluentValidation;
using Microsoft.Extensions.Logging;
using Rested.Core.Data;
using Rested.Core.MediatR.Commands;
using Rested.Core.MediatR.Validation;
using Rested.Mongo.Data;
using Rested.Mongo.MediatR.Commands;
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
                When(command => command.Action is not CommandActions.Patch and not CommandActions.Prune and not CommandActions.Delete, () =>
                {
                    RuleFor(command => command.Dto.Data.FirstName)
                        .NotEmpty()
                        .WithServiceErrorCode(ServiceErrorCodes.FirstNameIsRequired);

                    RuleFor(command => command.Dto.Data.LastName)
                        .NotEmpty()
                        .WithServiceErrorCode(ServiceErrorCodes.LastNameIsRequired);
                });
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

        protected override async void OnSetPrecalculatedProperties(EmployeeDtoCommand command, MongoDocument<Employee> document)
        {
            if (command.Action is CommandActions.Patch)
            {
                var originalDocument = await GetDocument(document.Id);

                var firstName = document?.Data?.FirstName is null ?
                    originalDocument?.Data?.FirstName :
                    document?.Data?.FirstName;

                var lastName = document?.Data?.LastName is null ?
                    originalDocument?.Data?.LastName :
                    document?.Data?.LastName;

                document.Data.FullName = $"{firstName} {lastName}";
            }

            else
            {
                document.Data.FullName = $"{document?.Data?.FirstName} {document?.Data?.LastName}";
                document.Data.Metadata = "Test Metadata that cannot be searched";
            }
        }

        #endregion Methods
    }
}

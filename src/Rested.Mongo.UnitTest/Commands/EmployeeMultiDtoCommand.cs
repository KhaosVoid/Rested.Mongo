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
    public class EmployeeMultiDtoCommand : MultiMongoDtoCommand<Employee>
    {
        #region Ctor

        public EmployeeMultiDtoCommand(
            List<Dto<Employee>> dtos,
            CommandActions action) :
                base(dtos, action)
        {

        }

        #endregion Ctor
    }

    public class EmployeeMultiDtoCommandValidator : MultiMongoDtoCommandValidator<Employee, EmployeeMultiDtoCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeMultiDtoCommandValidator() : base()
        {
            When(
                predicate: command => command.Action is CommandActions.Insert or CommandActions.Update,
                action: () =>
                {
                    RuleForEach(command => command.Dtos).ChildRules(dtos =>
                    {
                        dtos.When(
                            predicate: dto => dto.Data is not null,
                            action: () =>
                            {
                                dtos.RuleFor(dto => dto.Data.FirstName)
                                    .NotEmpty()
                                    .WithServiceErrorCode(ServiceErrorCodes.FirstNameIsRequired);

                                dtos.RuleFor(dto => dto.Data.LastName)
                                    .NotEmpty()
                                    .WithServiceErrorCode(ServiceErrorCodes.LastNameIsRequired);
                            });
                    });
                });

            When(
                predicate: command => command.Action is CommandActions.Patch,
                action: () =>
                {
                    RuleForEach(command => command.Dtos).ChildRules(dtos =>
                    {
                        dtos.When(
                            predicate: dto => dto.Data is not null,
                            action: () =>
                            {
                                dtos.When(
                                    predicate: dto => dto.Data.FirstName is not null,
                                    action: () =>
                                    {
                                        dtos.RuleFor(dto => dto.Data.FirstName)
                                            .NotEmpty()
                                            .WithServiceErrorCode(ServiceErrorCodes.FirstNameIsRequired);
                                    });

                                dtos.When(
                                    predicate: dto => dto.Data.LastName is not null,
                                    action: () =>
                                    {
                                        dtos.RuleFor(dto => dto.Data.LastName)
                                            .NotEmpty()
                                            .WithServiceErrorCode(ServiceErrorCodes.LastNameIsRequired);
                                    });
                            });
                    });
                });
        }

        #endregion Ctor
    }

    public class EmployeeMultiDtoCommandHandler : MultiMongoDtoCommandHandler<Employee, EmployeeMultiDtoCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeMultiDtoCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor

        #region Methods

        protected override async void OnSetPrecalculatedProperties(EmployeeMultiDtoCommand command, IEnumerable<MongoDocument<Employee>> documents)
        {
            if (command.Action is CommandActions.Patch)
            {
                var originalDocuments = await GetDocuments(documents.Select(mongoDocument => mongoDocument.Id).ToArray());

                foreach (var document in documents)
                {
                    var originalDocument = originalDocuments.FirstOrDefault(d => d.Id == document.Id);

                    if (originalDocument is null)
                        continue;

                    var firstName = document?.Data?.FirstName is null ?
                        originalDocument?.Data?.FirstName :
                        document?.Data?.FirstName;

                    var lastName = document?.Data?.LastName is null ?
                        originalDocument?.Data?.LastName :
                        document?.Data?.LastName;

                    document.Data.FullName = $"{firstName} {lastName}";
                }
            }

            else
            {
                foreach (var document in documents)
                {
                    document.Data.FullName = $"{document?.Data?.FirstName} {document?.Data?.LastName}";
                    document.Data.Metadata = "Test Metadata that cannot be searched";
                }
            }
        }

        #endregion Methods
    }
}

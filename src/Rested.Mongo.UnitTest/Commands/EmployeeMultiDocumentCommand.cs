using Microsoft.Extensions.Logging;
using Rested.Core.MediatR.Commands;
using Rested.Mongo.Data;
using Rested.Mongo.MediatR.Commands;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Commands
{
    public class EmployeeMultiDocumentCommand : MultiMongoDocumentCommand<Employee>
    {
        #region Ctor

        public EmployeeMultiDocumentCommand(
            List<MongoDocument<Employee>> documents,
            CommandActions action) :
                base(documents, action)
        {

        }

        #endregion Ctor
    }

    public class EmployeeMultiDocumentCommandValidator : MultiMongoDocumentCommandValidator<Employee, EmployeeMultiDocumentCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeMultiDocumentCommandValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class EmployeeMultiDocumentCommandHandler : MultiMongoDocumentCommandHandler<Employee, EmployeeMultiDocumentCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeMultiDocumentCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

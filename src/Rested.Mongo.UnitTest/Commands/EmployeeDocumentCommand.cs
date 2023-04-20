using Microsoft.Extensions.Logging;
using Rested.Core.CQRS.Commands;
using Rested.Mongo.CQRS.Commands;
using Rested.Mongo.CQRS.Data;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Commands
{
    public class EmployeeDocumentCommand : MongoDocumentCommand<Employee>
    {
        #region Ctor

        public EmployeeDocumentCommand(
            MongoDocument<Employee> document,
            CommandActions action) :
                base(document, action)
        {

        }

        #endregion Ctor
    }

    public class EmployeeDocumentCommandValidator : MongoDocumentCommandValidator<Employee, EmployeeDocumentCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeDocumentCommandValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class EmployeeDocumentCommandHandler : MongoDocumentCommandHandler<Employee, EmployeeDocumentCommand>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public EmployeeDocumentCommandHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

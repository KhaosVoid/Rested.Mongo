using Microsoft.Extensions.Logging;
using Rested.Mongo.CQRS.Data;
using Rested.Mongo.CQRS.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Queries
{
    public class GetEmployeeDocumentQuery : GetMongoDocumentQuery<Employee>
    {
        #region Ctor

        public GetEmployeeDocumentQuery(Guid id) : base(id)
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeDocumentQueryValidator : GetMongoDocumentQueryValidator<Employee, GetEmployeeDocumentQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeDocumentQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeDocumentQueryHandler : GetMongoDocumentQueryHandler<Employee, GetEmployeeDocumentQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeDocumentQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

using Microsoft.Extensions.Logging;
using Rested.Core.Validation;
using Rested.Mongo.Data;
using Rested.Mongo.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Queries
{
    public class GetEmployeeDocumentsQuery : GetMongoDocumentsQuery<Employee>
    {
        #region Ctor

        public GetEmployeeDocumentsQuery()
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeDocumentsQueryValidator : GetMongoDocumentsQueryValidator<Employee, GetEmployeeDocumentsQuery>
    {
        #region Properties

        public override ServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeDocumentsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeDocumentsQueryHandler : GetMongoDocumentsQueryHandler<Employee, GetEmployeeDocumentsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeDocumentsQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

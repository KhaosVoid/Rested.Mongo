using Microsoft.Extensions.Logging;
using Rested.Mongo.Data;
using Rested.Mongo.MediatR.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Queries
{
    public class GetEmployeeProjectionsQuery : GetMongoProjectionsQuery<Employee, EmployeeProjection>
    {
        #region Ctor

        public GetEmployeeProjectionsQuery() : base()
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeProjectionsQueryValidator : GetMongoProjectionsQueryValidator<Employee, EmployeeProjection, GetEmployeeProjectionsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeProjectionsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeProjectionsQueryHandler : GetMongoProjectionsQueryHandler<Employee, EmployeeProjection, GetEmployeeProjectionsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeProjectionsQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

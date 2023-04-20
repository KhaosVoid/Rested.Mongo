using Microsoft.Extensions.Logging;
using Rested.Mongo.CQRS.Data;
using Rested.Mongo.CQRS.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Queries
{
    public class GetEmployeeProjectionQuery : GetMongoProjectionQuery<Employee, EmployeeProjection>
    {
        #region Ctor

        public GetEmployeeProjectionQuery(Guid id) : base(id)
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeProjectionQueryValidator : GetMongoProjectionQueryValidator<Employee, EmployeeProjection, GetEmployeeProjectionQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeProjectionQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class GetEmployeeProjectionQueryHandler : GetMongoProjectionQueryHandler<Employee, EmployeeProjection, GetEmployeeProjectionQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public GetEmployeeProjectionQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

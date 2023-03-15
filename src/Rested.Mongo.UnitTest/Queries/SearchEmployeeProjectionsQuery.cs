using Microsoft.Extensions.Logging;
using Rested.Core.Data;
using Rested.Mongo.Data;
using Rested.Mongo.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Queries
{
    public class SearchEmployeeProjectionsQuery : SearchMongoProjectionsQuery<Employee, EmployeeProjection>
    {
        #region Ctor

        public SearchEmployeeProjectionsQuery(SearchRequest searchRequest) : base(searchRequest)
        {

        }

        #endregion Ctor
    }

    public class SearchEmployeeProjectionsQueryValidator : SearchMongoProjectionsQueryValidator<Employee, EmployeeProjection, SearchEmployeeProjectionsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public SearchEmployeeProjectionsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class SearchEmployeeProjectionsQueryHandler : SearchMongoProjectionsQueryHandler<Employee, EmployeeProjection, SearchEmployeeProjectionsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public SearchEmployeeProjectionsQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

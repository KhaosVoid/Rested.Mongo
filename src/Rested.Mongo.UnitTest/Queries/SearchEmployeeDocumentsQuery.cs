using Microsoft.Extensions.Logging;
using Rested.Core.Data;
using Rested.Mongo.Data;
using Rested.Mongo.MediatR.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Validation;

namespace Rested.Mongo.UnitTest.Queries
{
    public class SearchEmployeeDocumentsQuery : SearchMongoDocumentsQuery<Employee>
    {
        #region Ctor

        public SearchEmployeeDocumentsQuery(SearchRequest searchRequest) : base(searchRequest)
        {

        }

        #endregion Ctor
    }

    public class SearchEmployeeDocumentsQueryValidator : SearchMongoDocumentsQueryValidator<Employee, SearchEmployeeDocumentsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public SearchEmployeeDocumentsQueryValidator() : base()
        {

        }

        #endregion Ctor
    }

    public class SearchEmployeeDocumentsQueryHandler : SearchMongoDocumentsQueryHandler<Employee, SearchEmployeeDocumentsQuery>
    {
        #region Properties

        public override EmployeeServiceErrorCodes ServiceErrorCodes => EmployeeServiceErrorCodes.Instance;

        #endregion Properties

        #region Ctor

        public SearchEmployeeDocumentsQueryHandler(
            ILoggerFactory loggerFactory,
            IMongoContext mongoContext) :
                base(loggerFactory, mongoContext)
        {

        }

        #endregion Ctor
    }
}

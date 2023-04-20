using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rested.Core.CQRS.Data;
using Rested.Mongo.Controllers;
using Rested.Mongo.CQRS.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Queries;

namespace Rested.Mongo.UnitTest.Controllers
{
    [ApiController]
    [Route("/sampleMongoApi")]
    public class EmployeeProjectionController : MongoProjectionController<Employee, EmployeeProjection>
    {
        #region Ctor
        
        public EmployeeProjectionController(
            IMediator mediator,
            IHttpContextAccessor httpContext,
            ILoggerFactory loggerFactory) :
                base(mediator, httpContext, loggerFactory)
        {

        }

        #endregion Ctor

        #region Methods

        protected override GetMongoProjectionQuery<Employee, EmployeeProjection> CreateGetMongoProjectionQuery(Guid id) =>
            new GetEmployeeProjectionQuery(id);

        protected override GetMongoProjectionsQuery<Employee, EmployeeProjection> CreateGetMongoProjectionsQuery() =>
            new GetEmployeeProjectionsQuery();

        protected override SearchMongoProjectionsQuery<Employee, EmployeeProjection> CreateSearchMongoProjectionsQuery(SearchRequest searchRequest) =>
            new SearchEmployeeProjectionsQuery(searchRequest);

        #endregion Methods
    }
}

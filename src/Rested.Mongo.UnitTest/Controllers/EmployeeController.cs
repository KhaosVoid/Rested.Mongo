﻿using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rested.Core.CQRS.Commands;
using Rested.Core.CQRS.Data;
using Rested.Mongo.Controllers;
using Rested.Mongo.CQRS.Commands;
using Rested.Mongo.CQRS.Queries;
using Rested.Mongo.UnitTest.Commands;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Queries;

namespace Rested.Mongo.UnitTest.Controllers
{
    [ApiController]
    [Route("/sampleMongoApi")]
    public class EmployeeController : MongoDocumentController<Employee>
    {
        #region Ctor

        public EmployeeController(
            IMediator mediator,
            IHttpContextAccessor httpContext,
            ILoggerFactory loggerFactory) :
                base(mediator, httpContext, loggerFactory)
        {

        }

        #endregion Ctor

        #region Methods

        protected override GetMongoDocumentQuery<Employee> CreateGetMongoDocumentQuery(Guid id) => new GetEmployeeDocumentQuery(id);
        protected override GetMongoDocumentsQuery<Employee> CreateGetMongoDocumentsQuery() => new GetEmployeeDocumentsQuery();
        protected override SearchMongoDocumentsQuery<Employee> CreateSearchMongoDocumentsQuery(SearchRequest searchRequest) => new SearchEmployeeDocumentsQuery(searchRequest);
        protected override MongoDtoCommand<Employee> CreateDtoCommand(Dto<Employee> dto, CommandActions action) => new EmployeeDtoCommand(dto, action);
        protected override MultiMongoDtoCommand<Employee> CreateMultiDtoCommand(List<Dto<Employee>> dtos, CommandActions action) => new EmployeeMultiDtoCommand(dtos, action);

        #endregion Methods
    }
}

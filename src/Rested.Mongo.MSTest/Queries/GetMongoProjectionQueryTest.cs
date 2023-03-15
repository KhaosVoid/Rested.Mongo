using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.Data;
using Rested.Core.MSTest.Queries;
using Rested.Mongo.Data;
using Rested.Mongo.Queries;
using System.Linq.Expressions;

namespace Rested.Mongo.MSTest.Queries
{
    public abstract class GetMongoProjectionQueryTest<TData, TProjection, TGetMongoProjectionQuery, TGetMongoProjectionQueryValidator, TGetMongoProjectionQueryHandler> :
        GetProjectionQueryTest<TData, MongoDocument<TData>, TProjection, TGetMongoProjectionQuery, TGetMongoProjectionQueryValidator, TGetMongoProjectionQueryHandler>
        where TData : IData
        where TProjection : Projection
        where TGetMongoProjectionQuery : GetMongoProjectionQuery<TData, TProjection>
        where TGetMongoProjectionQueryValidator : GetMongoProjectionQueryValidator<TData, TProjection, TGetMongoProjectionQuery>
        where TGetMongoProjectionQueryHandler : GetMongoProjectionQueryHandler<TData, TProjection, TGetMongoProjectionQuery>
    {
        #region Members

        protected IMongoContext _mongoContextMock;

        #endregion Members

        #region Initialization

        protected override void OnInitializeMockDependencies()
        {
            base.OnInitializeMockDependencies();

            _mongoContextMock = Substitute.For<IMongoContext>();
        }

        protected override void OnInitializeTestDocument()
        {
            TestDocument = CreateDocument(data: InitializeTestData());
        }

        protected override void OnInitializeTestProjection()
        {
            TestProjection = CreateProjection(TestDocument);
        }

        #endregion Initialization

        #region Methods

        protected override MongoDocument<T> CreateDocument<T>(T data = default)
        {
            return new MongoDocument<T>()
            {
                Id = Guid.NewGuid(),
                ETag = BitConverter.GetBytes(0UL),
                CreateDateTime = DateTime.Now,
                CreateUser = "UnitTestUser",
                UpdateDateTime = DateTime.Now,
                UpdateUser = "UnitTestUser",
                Data = data
            };
        }

        protected override TGetMongoProjectionQuery CreateGetProjectionQuery()
        {
            return (TGetMongoProjectionQuery)Activator.CreateInstance(
                type: typeof(TGetMongoProjectionQuery),
                args: new object[] { TestDocument.Id });
        }

        protected override TGetMongoProjectionQueryValidator CreateGetProjectionQueryValidator()
        {
            return Activator.CreateInstance<TGetMongoProjectionQueryValidator>();
        }

        protected override TGetMongoProjectionQueryHandler CreateGetProjectionQueryHandler()
        {
            return (TGetMongoProjectionQueryHandler)Activator.CreateInstance(
                type: typeof(TGetMongoProjectionQueryHandler),
                args: new object[] { _loggerFactoryMock, _mongoContextMock });
        }

        protected virtual MongoQueryException CreateMockMongoQueryException()
        {
            var connectionId = new MongoDB.Driver.Core.Connections.ConnectionId(
                new MongoDB.Driver.Core.Servers.ServerId(
                    clusterId: new MongoDB.Driver.Core.Clusters.ClusterId(1),
                    endPoint: new System.Net.IPEndPoint(0x0, 0x0)));

            return Substitute.For<MongoQueryException>(connectionId, "Test MongoDB Network Timeout error.", null, null);
        }

        protected virtual void TestMongoQueryException()
        {
            try
            {
                ExecuteQuery(CreateMockMongoQueryException());
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<AggregateException>();
                ex.As<AggregateException>().InnerException.Should().BeOfType<ValidationException>();
                ex.As<AggregateException>().InnerException.As<ValidationException>().Errors.Count().Should().Be(1);
            }
        }

        protected virtual void TestException()
        {
            var exception = new Exception("Test Exception");

            try
            {
                ExecuteQuery(exception);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<AggregateException>();
                ex.As<AggregateException>().InnerException.Message.Should().Be(exception.Message);
            }
        }

        protected virtual TProjection ExecuteQuery(Exception failWithException = null)
        {
            var getProjectionQuery = CreateGetProjectionQuery();
            var getProjectionQueryValidator = CreateGetProjectionQueryValidator();
            var getProjectionQueryHandler = CreateGetProjectionQueryHandler();

            var validationResult = getProjectionQueryValidator.Validate(getProjectionQuery);

            var validationErrorMessage = validationResult.Errors?.Count > 0 ?
                validationResult.Errors.First().ErrorMessage : "";

            validationResult.IsValid.Should().BeTrue(because: validationErrorMessage);

            if (failWithException is not null)
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .GetProjectionAsync(
                        predicate: Arg.Any<Expression<Func<MongoDocument<TData>, bool>>>(),
                        projectionDefinition: Arg.Any<ProjectionDefinition<MongoDocument<TData>, TProjection>>())
                    .ReturnsForAnyArgs(Task.FromException<TProjection>(failWithException));
            }

            else
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .GetProjectionAsync(
                        predicate: Arg.Any<Expression<Func<MongoDocument<TData>, bool>>>(),
                        projectionDefinition: Arg.Any<ProjectionDefinition<MongoDocument<TData>, TProjection>>())
                    .ReturnsForAnyArgs(TestProjection);
            }

            return getProjectionQueryHandler.Handle(getProjectionQuery, new CancellationToken()).Result;
        }

        #endregion Methods

        #region Query Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void GetProjectionById()
        {
            var response = ExecuteQuery();

            response.Should().NotBeNull(because: ASSERTMSG_QUERY_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Should().BeEquivalentTo(TestProjection);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestMongoQueryExceptionOnGetProjectionById()
        {
            TestMongoQueryException();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestExceptionOnGetProjectionById()
        {
            TestException();
        }

        #endregion Query Tests
    }
}

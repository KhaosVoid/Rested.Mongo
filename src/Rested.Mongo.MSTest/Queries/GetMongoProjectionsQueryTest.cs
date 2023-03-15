using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.Data;
using Rested.Core.MSTest.Queries;
using Rested.Mongo.Data;
using Rested.Mongo.Queries;

namespace Rested.Mongo.MSTest.Queries
{
    public abstract class GetMongoProjectionsQueryTest<TData, TProjection, TGetMongoProjectionsQuery, TGetMongoProjectionsQueryValidator, TGetMongoProjectionsQueryHandler> :
        GetProjectionsQueryTest<TData, MongoDocument<TData>, TProjection, TGetMongoProjectionsQuery, TGetMongoProjectionsQueryValidator, TGetMongoProjectionsQueryHandler>
        where TData : IData
        where TProjection : Projection
        where TGetMongoProjectionsQuery : GetMongoProjectionsQuery<TData, TProjection>
        where TGetMongoProjectionsQueryValidator : GetMongoProjectionsQueryValidator<TData, TProjection, TGetMongoProjectionsQuery>
        where TGetMongoProjectionsQueryHandler : GetMongoProjectionsQueryHandler<TData, TProjection, TGetMongoProjectionsQuery>
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

        protected override void OnInitializeTestDocuments()
        {
            TestDocuments = InitializeTestData().Select(CreateDocument).ToList();
        }

        protected override void OnInitializeTestProjections()
        {
            TestProjections = TestDocuments.Select(CreateProjection).ToList();
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

        protected override TGetMongoProjectionsQuery CreateGetProjectionsQuery()
        {
            return Activator.CreateInstance<TGetMongoProjectionsQuery>();
        }

        protected override TGetMongoProjectionsQueryValidator CreateGetProjectionsQueryValidator()
        {
            return Activator.CreateInstance<TGetMongoProjectionsQueryValidator>();
        }

        protected override TGetMongoProjectionsQueryHandler CreateGetProjectionsQueryHandler()
        {
            return (TGetMongoProjectionsQueryHandler)Activator.CreateInstance(
                type: typeof(TGetMongoProjectionsQueryHandler),
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

        protected virtual List<TProjection> ExecuteQuery(Exception failWithException = null)
        {
            var getProjectionsQuery = CreateGetProjectionsQuery();
            var getProjectionsQueryValidator = CreateGetProjectionsQueryValidator();
            var getProjectionsQueryHandler = CreateGetProjectionsQueryHandler();

            var validationResult = getProjectionsQueryValidator.Validate(getProjectionsQuery);

            var validationErrorMessage = validationResult.Errors?.Count > 0 ?
                validationResult.Errors.First().ErrorMessage : "";

            validationResult.IsValid.Should().BeTrue(because: validationErrorMessage);

            if (failWithException is not null)
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .FindProjectionsAsync(
                        filterDefinition: Arg.Any<FilterDefinition<MongoDocument<TData>>>(),
                        projectionDefinition: Arg.Any<ProjectionDefinition<MongoDocument<TData>, TProjection>>())
                    .ReturnsForAnyArgs(Task.FromException<IEnumerable<TProjection>>(failWithException));
            }

            else
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .FindProjectionsAsync(
                        filterDefinition: Arg.Any<FilterDefinition<MongoDocument<TData>>>(),
                        projectionDefinition: Arg.Any<ProjectionDefinition<MongoDocument<TData>, TProjection>>())
                    .ReturnsForAnyArgs(TestProjections);
            }

            return getProjectionsQueryHandler.Handle(getProjectionsQuery, new CancellationToken()).Result;
        }

        #endregion Methods

        #region Query Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void GetProjections()
        {
            var response = ExecuteQuery();

            response.Should().NotBeNullOrEmpty(because: ASSERTMSG_QUERY_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Count.Should().Be(TestProjections.Count);
            response.Should().BeEquivalentTo(TestProjections);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestMongoQueryExceptionOnGetProjections()
        {
            TestMongoQueryException();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestExceptionOnGetProjections()
        {
            TestException();
        }

        #endregion Query Tests
    }
}

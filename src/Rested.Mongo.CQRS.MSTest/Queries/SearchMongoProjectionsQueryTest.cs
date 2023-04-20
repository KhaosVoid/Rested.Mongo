using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.CQRS.Data;
using Rested.Core.MSTest.Queries;
using Rested.Mongo.CQRS.Data;
using Rested.Mongo.CQRS.Queries;

namespace Rested.Mongo.CQRS.MSTest.Queries
{
    public abstract class SearchMongoProjectionsQueryTest<TData, TProjection, TSearchMongoProjectionsQuery, TSearchMongoProjectionsQueryValidator, TSearchMongoProjectionsQueryHandler> :
        SearchProjectionsQueryTest<TData, MongoDocument<TData>, TProjection, TSearchMongoProjectionsQuery, TSearchMongoProjectionsQueryValidator, TSearchMongoProjectionsQueryHandler>
        where TData : IData
        where TProjection : Projection
        where TSearchMongoProjectionsQuery : SearchMongoProjectionsQuery<TData, TProjection>
        where TSearchMongoProjectionsQueryValidator : SearchMongoProjectionsQueryValidator<TData, TProjection, TSearchMongoProjectionsQuery>
        where TSearchMongoProjectionsQueryHandler : SearchMongoProjectionsQueryHandler<TData, TProjection, TSearchMongoProjectionsQuery>
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

        protected override TSearchMongoProjectionsQuery CreateSearchProjectionsQuery(SearchRequest searchRequest)
        {
            return (TSearchMongoProjectionsQuery)Activator.CreateInstance(
                type: typeof(TSearchMongoProjectionsQuery),
                args: new object[] { searchRequest });
        }

        protected override TSearchMongoProjectionsQueryValidator CreateSearchProjectionsQueryValidator()
        {
            return Activator.CreateInstance<TSearchMongoProjectionsQueryValidator>();
        }

        protected override TSearchMongoProjectionsQueryHandler CreateSearchProjectionsQueryHandler()
        {
            return (TSearchMongoProjectionsQueryHandler)Activator.CreateInstance(
                type: typeof(TSearchMongoProjectionsQueryHandler),
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
            var searchRequest = new SearchRequest()
            {
                Page = 1
            };

            try
            {
                ExecuteQuery(searchRequest, CreateMockMongoQueryException());
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<AggregateException>();
                ex.As<AggregateException>().InnerExceptions.Count.Should().Be(1);
            }
        }

        protected virtual void TestException()
        {
            var exception = new Exception("Test Exception");
            var searchRequest = new SearchRequest()
            {
                Page = 1
            };

            try
            {
                ExecuteQuery(searchRequest, exception);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<AggregateException>();
                ex.As<AggregateException>().InnerException.InnerException.Message.Should().Be(exception.Message);
            }
        }

        protected virtual SearchProjectionsResults<TData, TProjection> ExecuteQuery(SearchRequest searchRequest = null, Exception failWithException = null)
        {
            var searchProjectionsQuery = CreateSearchProjectionsQuery(searchRequest);
            var searchProjectionsQueryValidator = CreateSearchProjectionsQueryValidator();
            var searchProjectionsQueryHandler = CreateSearchProjectionsQueryHandler();

            var validationResult = searchProjectionsQueryValidator.Validate(searchProjectionsQuery);

            var validationErrorMessage = validationResult.Errors?.Count > 0 ?
                validationResult.Errors.First().ErrorMessage : "";

            validationResult.IsValid.Should().BeTrue(because: validationErrorMessage);

            if (failWithException is not null)
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .Collection
                    .FindAsync(
                        filter: Arg.Any<FilterDefinition<MongoDocument<TData>>>(),
                        options: Arg.Any<FindOptions<MongoDocument<TData>, TProjection>>())
                    .ReturnsForAnyArgs(Task.FromException<IAsyncCursor<TProjection>>(failWithException));

                return searchProjectionsQueryHandler.Handle(searchProjectionsQuery, new CancellationToken()).Result;
            }

            else
            {
                var queryHandlerMock = Substitute.For<SearchMongoProjectionsQueryHandler<TData, TProjection, TSearchMongoProjectionsQuery>>(_loggerFactoryMock, _mongoContextMock);

                queryHandlerMock
                    .Handle(
                        query: Arg.Any<TSearchMongoProjectionsQuery>(),
                        cancellationToken: Arg.Any<CancellationToken>())
                    .ReturnsForAnyArgs(
                        new SearchProjectionsResults<TData, TProjection>(searchRequest)
                        {
                            TotalPages = 1,
                            TotalQueriedRecords = TestProjections.Count,
                            TotalRecords = TestProjections.Count,
                            Items = TestProjections
                        });

                return queryHandlerMock.Handle(searchProjectionsQuery, new CancellationToken()).Result;
            }
        }

        #endregion Methods

        #region Query Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void SearchProjections()
        {
            var searchRequest = new SearchRequest()
            {
                Page = 1
            };

            var response = ExecuteQuery(searchRequest);

            response.Should().NotBeNull(because: ASSERTMSG_QUERY_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Items.Count.Should().Be(TestProjections.Count);
            response.Items.Should().BeEquivalentTo(TestProjections);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestMongoQueryExceptionOnSearchProjections()
        {
            TestMongoQueryException();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestExceptionOnSearchProjections()
        {
            TestException();
        }

        #endregion Query Tests
    }
}

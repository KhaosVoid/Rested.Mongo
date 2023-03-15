using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.Data;
using Rested.Core.MSTest.Queries;
using Rested.Mongo.Data;
using Rested.Mongo.Queries;

namespace Rested.Mongo.MSTest.Queries
{
    public abstract class SearchMongoDocumentsQueryTest<TData, TSearchMongoDocumentsQuery, TSearchMongoDocumentsQueryValidator, TSearchMongoDocumentsQueryHandler> :
        SearchDocumentsQueryTest<TData, MongoDocument<TData>, TSearchMongoDocumentsQuery, TSearchMongoDocumentsQueryValidator, TSearchMongoDocumentsQueryHandler>
        where TData : IData
        where TSearchMongoDocumentsQuery : SearchMongoDocumentsQuery<TData>
        where TSearchMongoDocumentsQueryValidator : SearchMongoDocumentsQueryValidator<TData, TSearchMongoDocumentsQuery>
        where TSearchMongoDocumentsQueryHandler : SearchMongoDocumentsQueryHandler<TData, TSearchMongoDocumentsQuery>
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

        protected override TSearchMongoDocumentsQuery CreateSearchDocumentsQuery(SearchRequest searchRequest)
        {
            return (TSearchMongoDocumentsQuery)Activator.CreateInstance(
                type: typeof(TSearchMongoDocumentsQuery),
                args: new object[] { searchRequest });
        }

        protected override TSearchMongoDocumentsQueryValidator CreateSearchDocumentsQueryValidator()
        {
            return Activator.CreateInstance<TSearchMongoDocumentsQueryValidator>();
        }

        protected override TSearchMongoDocumentsQueryHandler CreateSearchDocumentsQueryHandler()
        {
            return (TSearchMongoDocumentsQueryHandler)Activator.CreateInstance(
                type: typeof(TSearchMongoDocumentsQueryHandler),
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

        protected virtual SearchDocumentsResults<TData, MongoDocument<TData>> ExecuteQuery(SearchRequest searchRequest = null, Exception failWithException = null)
        {
            var searchDocumentsQuery = CreateSearchDocumentsQuery(searchRequest);
            var searchDocumentsQueryValidator = CreateSearchDocumentsQueryValidator();
            var searchDocumentsQueryHandler = CreateSearchDocumentsQueryHandler();

            var validationResult = searchDocumentsQueryValidator.Validate(searchDocumentsQuery);

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
                        options: Arg.Any<FindOptions<MongoDocument<TData>>>())
                    .ReturnsForAnyArgs(Task.FromException<IAsyncCursor<MongoDocument<TData>>>(failWithException));

                return searchDocumentsQueryHandler.Handle(searchDocumentsQuery, new CancellationToken()).Result;
            }

            else
            {
                var queryHandlerMock = Substitute.For<SearchMongoDocumentsQueryHandler<TData, TSearchMongoDocumentsQuery>>(_loggerFactoryMock, _mongoContextMock);

                queryHandlerMock
                    .Handle(
                        query: Arg.Any<TSearchMongoDocumentsQuery>(),
                        cancellationToken: Arg.Any<CancellationToken>())
                    .ReturnsForAnyArgs(
                        new SearchDocumentsResults<TData, MongoDocument<TData>>(searchRequest)
                        {
                            TotalPages = 1,
                            TotalQueriedRecords = TestDocuments.Count,
                            TotalRecords = TestDocuments.Count,
                            Items = TestDocuments
                        });

                return queryHandlerMock.Handle(searchDocumentsQuery, new CancellationToken()).Result;
            }
        }

        #endregion Methods

        #region Query Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void SearchDocuments()
        {
            var searchRequest = new SearchRequest()
            {
                Page = 1
            };

            var response = ExecuteQuery(searchRequest);

            response.Should().NotBeNull(because: ASSERTMSG_QUERY_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Items.Count.Should().Be(TestDocuments.Count);
            response.Items.Should().BeEquivalentTo(TestDocuments);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestMongoQueryExceptionOnSearchDocuments()
        {
            TestMongoQueryException();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestExceptionOnSearchDocuments()
        {
            TestException();
        }

        #endregion Query Tests
    }
}

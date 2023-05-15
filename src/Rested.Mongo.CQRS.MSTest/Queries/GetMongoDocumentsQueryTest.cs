using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.CQRS.Data;
using Rested.Core.MSTest.Queries;
using Rested.Mongo.CQRS.Data;
using Rested.Mongo.CQRS.Queries;
using System.Linq.Expressions;

namespace Rested.Mongo.CQRS.MSTest.Queries
{
    public abstract class GetMongoDocumentsQueryTest<TData, TGetMongoDocumentsQuery, TGetMongoDocumentsQueryValidator, TGetMongoDocumentsQueryHandler> :
        GetDocumentsQueryTest<TData, MongoDocument<TData>, TGetMongoDocumentsQuery, TGetMongoDocumentsQueryValidator, TGetMongoDocumentsQueryHandler>
        where TData : IData
        where TGetMongoDocumentsQuery : GetMongoDocumentsQuery<TData>
        where TGetMongoDocumentsQueryValidator : GetMongoDocumentsQueryValidator<TData, TGetMongoDocumentsQuery>
        where TGetMongoDocumentsQueryHandler : GetMongoDocumentsQueryHandler<TData, TGetMongoDocumentsQuery>
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

        protected override TGetMongoDocumentsQuery CreateGetDocumentsQuery()
        {
            return Activator.CreateInstance<TGetMongoDocumentsQuery>();
        }

        protected override TGetMongoDocumentsQueryValidator CreateGetDocumentsQueryValidator()
        {
            return Activator.CreateInstance<TGetMongoDocumentsQueryValidator>();
        }

        protected override TGetMongoDocumentsQueryHandler CreateGetDocumentsQueryHandler()
        {
            return (TGetMongoDocumentsQueryHandler)Activator.CreateInstance(
                type: typeof(TGetMongoDocumentsQueryHandler),
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

        protected virtual List<MongoDocument<TData>> ExecuteQuery(Exception failWithException = null)
        {
            var getDocumentsQuery = CreateGetDocumentsQuery();
            var getDocumentsQueryValidator = CreateGetDocumentsQueryValidator();
            var getDocumentsQueryHandler = CreateGetDocumentsQueryHandler();

            var validationResult = getDocumentsQueryValidator.Validate(getDocumentsQuery);

            var validationErrorMessage = validationResult.Errors?.Count > 0 ?
                validationResult.Errors.First().ErrorMessage : "";

            validationResult.IsValid.Should().BeTrue(because: validationErrorMessage);

            if (failWithException is not null)
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .FindDocumentsAsync(Arg.Any<Expression<Func<MongoDocument<TData>, bool>>>())
                    .ReturnsForAnyArgs(Task.FromException<IEnumerable<MongoDocument<TData>>>(failWithException));
            }

            else
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .FindDocumentsAsync(Arg.Any<Expression<Func<MongoDocument<TData>, bool>>>())
                    .ReturnsForAnyArgs(TestDocuments);
            }

            return getDocumentsQueryHandler.Handle(getDocumentsQuery, new CancellationToken()).Result;
        }

        #endregion Methods

        #region Query Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void GetDocuments()
        {
            var response = ExecuteQuery();

            response.Should().NotBeNullOrEmpty(because: ASSERTMSG_QUERY_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Count.Should().Be(TestDocuments.Count);
            response.Should().BeEquivalentTo(TestDocuments);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestMongoQueryExceptionOnGetDocuments()
        {
            TestMongoQueryException();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestExceptionOnGetDocuments()
        {
            TestException();
        }

        #endregion Query Tests
    }
}

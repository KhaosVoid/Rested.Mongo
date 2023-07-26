using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.Data;
using Rested.Core.MediatR.MSTest.Queries;
using Rested.Mongo.Data;
using Rested.Mongo.MediatR.Queries;

namespace Rested.Mongo.MediatR.MSTest.Queries
{
    public abstract class GetMongoDocumentQueryTest<TData, TGetMongoDocumentQuery, TGetMongoDocumentQueryValidator, TGetMongoDocumentQueryHandler> :
        GetDocumentQueryTest<TData, MongoDocument<TData>, TGetMongoDocumentQuery, TGetMongoDocumentQueryValidator, TGetMongoDocumentQueryHandler>
        where TData : IData
        where TGetMongoDocumentQuery : GetMongoDocumentQuery<TData>
        where TGetMongoDocumentQueryValidator : GetMongoDocumentQueryValidator<TData, TGetMongoDocumentQuery>
        where TGetMongoDocumentQueryHandler : GetMongoDocumentQueryHandler<TData, TGetMongoDocumentQuery>
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

        protected override TGetMongoDocumentQuery CreateGetDocumentQuery()
        {
            return (TGetMongoDocumentQuery)Activator.CreateInstance(
                type: typeof(TGetMongoDocumentQuery),
                args: new object[] { TestDocument.Id });
        }

        protected override TGetMongoDocumentQueryValidator CreateGetDocumentQueryValidator()
        {
            return Activator.CreateInstance<TGetMongoDocumentQueryValidator>();
        }

        protected override TGetMongoDocumentQueryHandler CreateGetDocumentQueryHandler()
        {
            return (TGetMongoDocumentQueryHandler)Activator.CreateInstance(
                type: typeof(TGetMongoDocumentQueryHandler),
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

        protected virtual MongoDocument<TData> ExecuteQuery(Exception failWithException = null)
        {
            var getDocumentQuery = CreateGetDocumentQuery();
            var getDocumentQueryValidator = CreateGetDocumentQueryValidator();
            var getDocumentQueryHandler = CreateGetDocumentQueryHandler();

            var validationResult = getDocumentQueryValidator.Validate(getDocumentQuery);

            var validationErrorMessage = validationResult.Errors?.Count > 0 ?
                validationResult.Errors.First().ErrorMessage : "";

            validationResult.IsValid.Should().BeTrue(because: validationErrorMessage);

            if (failWithException is not null)
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .GetDocumentAsync(Arg.Any<Guid>())
                    .ReturnsForAnyArgs(Task.FromException<MongoDocument<TData>>(failWithException));
            }

            else
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .GetDocumentAsync(Arg.Any<Guid>())
                    .ReturnsForAnyArgs(TestDocument);
            }

            return getDocumentQueryHandler.Handle(getDocumentQuery, new CancellationToken()).Result;
        }

        #endregion Methods

        #region Query Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void GetDocumentById()
        {
            var response = ExecuteQuery();

            response.Should().NotBeNull(because: ASSERTMSG_QUERY_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Should().BeEquivalentTo(TestDocument);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestMongoQueryExceptionOnGetDocumentById()
        {
            TestMongoQueryException();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TESTS)]
        public virtual void TestExceptionOnGetDocumentById()
        {
            TestException();
        }

        #endregion Query Tests
    }
}

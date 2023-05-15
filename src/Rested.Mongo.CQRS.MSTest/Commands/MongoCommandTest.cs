using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using NSubstitute;
using Rested.Core.CQRS.Commands;
using Rested.Core.CQRS.Data;
using Rested.Core.CQRS.MSTest.Commands;
using Rested.Mongo.CQRS.Commands;
using Rested.Mongo.CQRS.Data;
using System.Reflection;

namespace Rested.Mongo.CQRS.MSTest.Commands
{
    public abstract class MongoCommandTest<TData, TMongoCommand, TMongoCommandValidator, TMongoCommandHandler> :
        DocumentCommandTest<TData, MongoDocument<TData>, TMongoCommand, TMongoCommandValidator, TMongoCommandHandler>
        where TData : IData
        where TMongoCommand : MongoCommand<TData>
        where TMongoCommandValidator : MongoCommandValidator<TData, TMongoCommand>
        where TMongoCommandHandler : MongoCommandHandler<TData, TMongoCommand>
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

        protected override TMongoCommandValidator CreateCommandValidator()
        {
            return Activator.CreateInstance<TMongoCommandValidator>();
        }

        protected override TMongoCommandHandler CreateCommandHandler()
        {
            return (TMongoCommandHandler)Activator.CreateInstance(
                type: typeof(TMongoCommandHandler),
                args: new object[] { _loggerFactoryMock, _mongoContextMock });
        }

        protected virtual MongoWriteException CreateMockMongoWriteException(MongoExceptionTypes mongoExceptionType)
        {
            var writeErrorParameters = mongoExceptionType switch
            {
                MongoExceptionTypes.IndexException => new object[] { ServerErrorCategory.DuplicateKey, 11000, $"index:{nameof(IIdentifiable.Id)}", new MongoDB.Bson.BsonDocument() },
                MongoExceptionTypes.WriteException => new object[] { ServerErrorCategory.ExecutionTimeout, 89, $"Test MongoDB Network Timeout error.", new MongoDB.Bson.BsonDocument() },
                _ => throw new NotImplementedException()
            };

            var writeError = typeof(WriteError)
                .GetTypeInfo()
                .DeclaredConstructors
                .First()
                .Invoke(writeErrorParameters) as WriteError;

            var connectionId = new MongoDB.Driver.Core.Connections.ConnectionId(
                new MongoDB.Driver.Core.Servers.ServerId(
                    clusterId: new MongoDB.Driver.Core.Clusters.ClusterId(1),
                    endPoint: new System.Net.IPEndPoint(0x0, 0x0)));

            return Substitute.For<MongoWriteException>(connectionId, writeError, null, null);
        }

        protected virtual void TestMongoWriteExceptionOnCommandAction(CommandActions action)
        {
            try
            {
                ExecuteCommandAction(
                    action: action,
                    failWithException: CreateMockMongoWriteException(MongoExceptionTypes.WriteException));
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<AggregateException>();
                ex.As<AggregateException>().InnerException.Should().BeOfType<ValidationException>();
                ex.As<AggregateException>().InnerException.As<ValidationException>().Errors.Count().Should().Be(1);
            }
        }

        protected virtual void TestExceptionOnCommandAction(CommandActions action)
        {
            var exception = new Exception("Test Exception.");

            try
            {
                ExecuteCommandAction(
                    action: action,
                    failWithException: exception);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<AggregateException>();
                ex.As<AggregateException>().InnerException.Message.Should().Be(exception.Message);
            }
        }

        protected virtual MongoDocument<TData> ExecuteCommandAction(CommandActions action, Exception failWithException = null)
        {
            var command = CreateCommand(action);
            var commandValidator = CreateCommandValidator();
            var commandHandler = CreateCommandHandler();

            var validationResult = commandValidator.Validate(command);

            var validationErrorMessage = validationResult.Errors?.Count > 0 ?
                validationResult.Errors.First().ErrorMessage : "";

            validationResult.IsValid.Should().BeTrue(because: validationErrorMessage);

            if (failWithException is not null)
            {
                _mongoContextMock
                    .SessionFactory
                    .CreateAsync()
                    .Result
                    .CommitTransactionAsync(Arg.Any<CancellationToken>())
                    .ReturnsForAnyArgs(Task.FromException(failWithException));
            }

            if (action is not CommandActions.Delete)
            {
                _mongoContextMock
                    .RepositoryFactory
                    .Create<TData>()
                    .GetDocumentAsync(Arg.Any<Guid>())
                    .ReturnsForAnyArgs(TestDocument);
            }

            return commandHandler.Handle(command, new CancellationToken()).Result;
        }

        #endregion Methods

        #region Insert Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_TESTS)]
        public virtual void Insert()
        {
            var response = ExecuteCommandAction(CommandActions.Insert);

            response.Should().NotBeNull(because: ASSERTMSG_COMMAND_RESPONSE_SHOULD_BE_NULL);
            response.Should().BeEquivalentTo(TestDocument);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_TESTS)]
        public virtual void TestMongoWriteExceptionOnInsert()
        {
            TestMongoWriteExceptionOnCommandAction(CommandActions.Insert);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_TESTS)]
        public virtual void TestExceptionOnInsert()
        {
            TestExceptionOnCommandAction(CommandActions.Insert);
        }

        #endregion Insert Tests

        #region Update Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_TESTS)]
        public virtual void Update()
        {
            var response = ExecuteCommandAction(CommandActions.Update);

            response.Should().NotBeNull(because: ASSERTMSG_COMMAND_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Should().BeEquivalentTo(TestDocument);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_TESTS)]
        public virtual void TestMongoWriteExceptionOnUpdate()
        {
            TestMongoWriteExceptionOnCommandAction(CommandActions.Update);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_TESTS)]
        public virtual void TestExceptionOnUpdate()
        {
            TestExceptionOnCommandAction(CommandActions.Update);
        }

        #endregion Update Tests

        #region Patch Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_TESTS)]
        public void Patch()
        {
            var response = ExecuteCommandAction(CommandActions.Patch);

            response.Should().NotBeNull(because: ASSERTMSG_COMMAND_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Should().BeEquivalentTo(TestDocument);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_TESTS)]
        public void TestMongoWriteExceptionOnPatch()
        {
            TestMongoWriteExceptionOnCommandAction(CommandActions.Patch);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_TESTS)]
        public void TestExceptionOnPatch()
        {
            TestExceptionOnCommandAction(CommandActions.Patch);
        }

        #endregion Patch Tests

        #region Prune Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_PRUNE_TESTS)]
        public void Prune()
        {
            var response = ExecuteCommandAction(CommandActions.Prune);

            response.Should().NotBeNull(because: ASSERTMSG_COMMAND_RESPONSE_SHOULD_NOT_BE_NULL);
            response.Should().BeEquivalentTo(TestDocument);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PRUNE_TESTS)]
        public void TestMongoWriteExceptionOnPrune()
        {
            TestMongoWriteExceptionOnCommandAction(CommandActions.Prune);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PRUNE_TESTS)]
        public void TestExceptionOnPrune()
        {
            TestExceptionOnCommandAction(CommandActions.Prune);
        }

        #endregion Prune Tests

        #region Delete Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_TESTS)]
        public virtual void Delete()
        {
            var response = ExecuteCommandAction(CommandActions.Delete);

            response.Should().BeNull(because: ASSERTMSG_COMMAND_RESPONSE_SHOULD_BE_NULL);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_TESTS)]
        public virtual void TestMongoWriteExceptionOnDelete()
        {
            TestMongoWriteExceptionOnCommandAction(CommandActions.Delete);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_TESTS)]
        public virtual void TestExceptionOnDelete()
        {
            TestExceptionOnCommandAction(CommandActions.Delete);
        }

        #endregion Delete Tests
    }
}

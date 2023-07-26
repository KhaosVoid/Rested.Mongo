using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Mongo.MediatR.MSTest.Commands;
using Rested.Mongo.UnitTest.Commands;
using Rested.Mongo.UnitTest.Data;

namespace Rested.Mongo.UnitTest.Tests.Commands
{
    [TestClass]
    public class EmployeeDocumentCommandTest :
        MongoDocumentCommandTest<Employee, EmployeeDocumentCommand, EmployeeDocumentCommandValidator, EmployeeDocumentCommandHandler>
    {
        #region Initialization

        protected override Employee InitializeTestData()
        {
            return new Employee()
            {
                FirstName = "FirstName",
                LastName = "LastName",
            };
        }

        #endregion Initialization
    }
}

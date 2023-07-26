using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Mongo.MediatR.MSTest.Commands;
using Rested.Mongo.UnitTest.Commands;
using Rested.Mongo.UnitTest.Data;

namespace Rested.Mongo.UnitTest.Tests.Commands
{
    [TestClass]
    public class EmployeeMultiDocumentCommandTest :
        MultiMongoDocumentCommandTest<Employee, EmployeeMultiDocumentCommand, EmployeeMultiDocumentCommandValidator, EmployeeMultiDocumentCommandHandler>
    {
        #region Initialization

        protected override List<Employee> InitializeTestData()
        {
            var testData = new List<Employee>();

            for (int i = 0; i < 3; i++)
            {
                testData.Add(new Employee()
                {
                    FirstName = $"FirstName{i:000}",
                    LastName = $"LastName{i:000}"
                });
            }

            return testData;
        }

        #endregion Initialization
    }
}

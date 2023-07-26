using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Mongo.Server.MSTest.Controllers;
using Rested.Mongo.UnitTest.Controllers;
using Rested.Mongo.UnitTest.Data;

namespace Rested.Mongo.UnitTest.Tests.Controllers
{
    [TestClass]
    public class EmployeeProjectionControllerTest :
        MongoProjectionControllerTest<Employee, EmployeeProjection, EmployeeProjectionController>
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

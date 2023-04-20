using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Mongo.CQRS.MSTest.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Queries;

namespace Rested.Mongo.UnitTest.Tests.Queries
{
    [TestClass]
    public class GetEmployeeDocumentsQueryTest :
        GetMongoDocumentsQueryTest<Employee, GetEmployeeDocumentsQuery, GetEmployeeDocumentsQueryValidator, GetEmployeeDocumentsQueryHandler>
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

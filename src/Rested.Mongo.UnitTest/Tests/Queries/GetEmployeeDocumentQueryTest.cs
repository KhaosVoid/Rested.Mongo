using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Mongo.CQRS.MSTest.Queries;
using Rested.Mongo.UnitTest.Data;
using Rested.Mongo.UnitTest.Queries;

namespace Rested.Mongo.UnitTest.Tests.Queries
{
    [TestClass]
    public class GetEmployeeDocumentQueryTest :
        GetMongoDocumentQueryTest<Employee, GetEmployeeDocumentQuery, GetEmployeeDocumentQueryValidator, GetEmployeeDocumentQueryHandler>
    {
        #region Initialization

        protected override Employee InitializeTestData()
        {
            return new Employee()
            {
                FirstName = "FirstName",
                LastName = "LastName"
            };
        }

        #endregion Initialization
    }
}

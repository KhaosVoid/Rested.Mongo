using Rested.Core.Data;
using Rested.Mongo.Data;

namespace Rested.Mongo.UnitTest.Data
{
    public class EmployeeProjection : Projection
    {
        #region Properties

        public Guid Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public DateTime DOB { get; set; }
        public DateOnly StartDate { get; set; }
        public EmploymentTypes EmploymentType { get; set; }

        #endregion Properties

        #region Ctor

        static EmployeeProjection()
        {
            RegisterMapping((EmployeeProjection p) => p.Id,             (MongoDocument<Employee> d) => d.Id);
            RegisterMapping((EmployeeProjection p) => p.FullName,       (MongoDocument<Employee> d) => d.Data.FullName);
            RegisterMapping((EmployeeProjection p) => p.Age,            (MongoDocument<Employee> d) => d.Data.Age);
            RegisterMapping((EmployeeProjection p) => p.DOB,            (MongoDocument<Employee> d) => d.Data.DOB);
            RegisterMapping((EmployeeProjection p) => p.StartDate,      (MongoDocument<Employee> d) => d.Data.StartDate);
            RegisterMapping((EmployeeProjection p) => p.EmploymentType, (MongoDocument<Employee> d) => d.Data.EmploymentType);
        }

        #endregion Ctor
    }
}

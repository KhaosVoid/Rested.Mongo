using Rested.Core.CQRS.Validation;
using Rested.Mongo.UnitTest.Data;
using System.Net;

namespace Rested.Mongo.UnitTest.Validation
{
    public class EmployeeServiceErrorCodes : ServiceErrorCodes
    {
        #region Properties

        public ServiceErrorCode FirstNameIsRequired => this[FIRST_NAME_IS_REQUIRED_NAME];
        public ServiceErrorCode LastNameIsRequired => this[LAST_NAME_IS_REQUIRED_NAME];

        public static EmployeeServiceErrorCodes Instance
        {
            get => _instance ??= new EmployeeServiceErrorCodes();
        }

        #endregion Properties

        #region Members

        private static EmployeeServiceErrorCodes? _instance;

        #endregion Members

        #region Error Code Names

        protected readonly string FIRST_NAME_IS_REQUIRED_NAME = "FirstNameIsRequired";
        protected readonly string LAST_NAME_IS_REQUIRED_NAME = "LastNameIsRequired";

        #endregion Error Code Names

        #region Error Code Messages

        protected readonly string FIRST_NAME_IS_REQUIRED_MESSAGE = $"The {nameof(Employee.FirstName)} is required.";
        protected readonly string LAST_NAME_IS_REQUIRED_MESSAGE = $"The {nameof(Employee.LastName)} is required.";

        #endregion Error Code Messages

        #region Ctor

        public EmployeeServiceErrorCodes() : base(serviceId: 1, featureId: 1)
        {

        }

        #endregion Ctor

        #region Properties

        protected override void OnInitializeCommonErrorCodesComplete()
        {
            OnInitializeEmployeeErrorCodes();
        }

        protected virtual void OnInitializeEmployeeErrorCodes()
        {
            Add(FIRST_NAME_IS_REQUIRED_NAME, FIRST_NAME_IS_REQUIRED_MESSAGE, HttpStatusCode.BadRequest);
            Add(LAST_NAME_IS_REQUIRED_NAME, LAST_NAME_IS_REQUIRED_MESSAGE, HttpStatusCode.BadRequest);
        }

        #endregion Properties
    }
}

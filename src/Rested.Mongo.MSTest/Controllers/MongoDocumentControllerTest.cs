using Rested.Core.Data;
using Rested.Core.MSTest.Controllers;
using Rested.Mongo.Controllers;
using Rested.Mongo.Data;

namespace Rested.Mongo.MSTest.Controllers
{
    public abstract class MongoDocumentControllerTest<TData, TMongoDocumentController> :
        DocumentControllerTest<TData, MongoDocument<TData>, TMongoDocumentController>
        where TData : IData
        where TMongoDocumentController : MongoDocumentController<TData>
    {
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

        #endregion Methods
    }
}

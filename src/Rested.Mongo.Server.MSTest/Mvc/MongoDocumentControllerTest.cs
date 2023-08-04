using Rested.Core.Data;
using Rested.Core.Server.MSTest.Mvc;
using Rested.Mongo.Data;
using Rested.Mongo.Server.Mvc;

namespace Rested.Mongo.Server.MSTest.Mvc
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

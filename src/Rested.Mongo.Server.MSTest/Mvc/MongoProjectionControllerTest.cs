using Rested.Core.Data;
using Rested.Core.Server.MSTest.Mvc;
using Rested.Mongo.Data;
using Rested.Mongo.Server.Mvc;

namespace Rested.Mongo.Server.MSTest.Mvc
{
    public abstract class MongoProjectionControllerTest<TData, TProjection, TMongoProjectionController> :
        ProjectionControllerTest<TData, MongoDocument<TData>, TProjection, TMongoProjectionController>
        where TData : IData
        where TProjection : Projection
        where TMongoProjectionController : MongoProjectionController<TData, TProjection>
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

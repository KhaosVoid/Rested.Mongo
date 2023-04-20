using MongoDB.Driver;

namespace Rested.Mongo.CQRS.Data
{
    public class MongoMultipleException : MongoException
    {
        #region Properties

        public List<MongoMultipleExceptionDetail> MongoExceptions { get; }

        #endregion Properties

        #region Ctor

        public MongoMultipleException(string message, List<MongoMultipleExceptionDetail> mongoExceptions) : base(message)
        {
            MongoExceptions = mongoExceptions;
        }

        #endregion Ctor
    }
}

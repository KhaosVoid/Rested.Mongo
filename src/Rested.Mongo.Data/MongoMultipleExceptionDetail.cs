namespace Rested.Mongo.Data
{
    public class MongoMultipleExceptionDetail
    {
        #region Properties

        public string CollectionName { get; }
        public Guid Id { get; }
        public ulong UpdateVersion { get; }
        public Exception Exception { get; }

        #endregion Properties

        #region Ctor

        public MongoMultipleExceptionDetail(string collectionName, Guid id, ulong updateVersion, Exception exception)
        {
            CollectionName = collectionName;
            Id = id;
            UpdateVersion = updateVersion;
            Exception = exception;
        }

        #endregion Ctor
    }
}

using MongoDB.Driver;

namespace Rested.Mongo.Server.Configuration
{
    public class BasicMongoDbSettings
    {
        #region Properties

        public string ServerHostName { get; set; }
        public int ServerPort { get; set; }
        public string DatabaseName { get; set; }

        #endregion Properties

        #region Methods

        public MongoClientSettings ToMongoClientSettings()
        {
            return new MongoClientSettings()
            {
                Server = new MongoServerAddress(ServerHostName, ServerPort)
            };
        }

        #endregion Methods
    }
}

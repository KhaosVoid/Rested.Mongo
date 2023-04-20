using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Rested.Mongo.CQRS.Data
{
    public sealed class MongoIndexViolationError
    {
        #region Constants

        private const string MONGODB_ERROR_CODE_NAME =
            @"(?<MongoErrorCode>E[0-9]+) (?<MongoErrorName>[\w\W]+(?= collection:))";

        private const string MONGODB_ERROR_COLLECTION_INDEX =
            @"(?<MongoCollectionIndex>(?<= index: )\w+)";

        //private const string MONGODB_ERROR_DUP_KEY_JSON =
        //    @"(?<MongoDupKeyJson>(?<= dup key: ){.*})";

        private const string MONGODB_ERROR_DUP_KEY_JSON =
            @"{\s(?<IndexName>.*):\s""(?<IndexValue>.*)""\s}";

        #endregion Constants

        #region Properties

        public string IndexName { get; }
        public string IndexValue { get; }

        #endregion Properties

        #region Ctor

        internal MongoIndexViolationError(string indexName, string indexValue)
        {
            IndexName = indexName;
            IndexValue = indexValue;
        }

        #endregion Ctor

        #region Methods

        public static bool TryParse(WriteError writeError, out MongoIndexViolationError mongoIndexViolationError)
        {
            mongoIndexViolationError = null;

            if (writeError is null || writeError.Code is not 11000)
                return false;

            var dupKeyJsonMatch = Regex.Match(
                input: writeError.Message,
                pattern: MONGODB_ERROR_DUP_KEY_JSON);

            if (!dupKeyJsonMatch.Success)
                return false;

            mongoIndexViolationError = new MongoIndexViolationError(
                indexName: dupKeyJsonMatch.Groups[nameof(IndexName)].Value,
                indexValue: dupKeyJsonMatch.Groups[nameof(IndexValue)].Value);

            return true;
        }

        #endregion Methods
    }
}

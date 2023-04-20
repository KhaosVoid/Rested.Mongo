using MongoDB.Bson;
using MongoDB.Driver;
using Rested.Core.CQRS.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rested.Mongo.CQRS.Data
{
    public static class Extensions
    {
        public static string GetCollectionName(this Type documentType)
        {
            var collectionName = documentType.GetCustomAttribute<BsonCollectionAttribute>()?.CollectionName;

            if (string.IsNullOrEmpty(collectionName))
                return documentType.Name;

            return collectionName;
        }

        private static BsonRegularExpression CreateBsonRegularExpression(string value, string regexFormat, bool isCaseSensitive)
        {
            string regexExpressionPattern = string.Format(regexFormat, Regex.Escape(value));

            if (isCaseSensitive)
                return new BsonRegularExpression(regexExpressionPattern);

            return new BsonRegularExpression(
                pattern: regexExpressionPattern,
                options: "i");
        }

        public static FilterDefinition<MongoDocument<TData>> Contains<TData>(
            this FilterDefinitionBuilder<MongoDocument<TData>> filterDefinitionBuilder,
            FieldDefinition<MongoDocument<TData>> fieldDefinition,
            string value,
            bool isCaseSensitive = false)
            where TData : IData
        {
            return filterDefinitionBuilder.Regex(
                field: fieldDefinition,
                regex: CreateBsonRegularExpression(
                    value: value,
                    regexFormat: "{0}",
                    isCaseSensitive: isCaseSensitive));
        }

        public static FilterDefinition<MongoDocument<TData>> EndsWith<TData>(
            this FilterDefinitionBuilder<MongoDocument<TData>> filterDefinitionBuilder,
            FieldDefinition<MongoDocument<TData>> fieldDefinition,
            string value,
            bool isCaseSensitive = false)
            where TData : IData
        {
            return filterDefinitionBuilder.Regex(
                field: fieldDefinition,
                regex: CreateBsonRegularExpression(
                    value: value,
                    regexFormat: "{0}$",
                    isCaseSensitive: isCaseSensitive));
        }

        public static FilterDefinition<MongoDocument<TData>> Match<TData>(
            this FilterDefinitionBuilder<MongoDocument<TData>> filterDefinitionBuilder,
            FieldDefinition<MongoDocument<TData>> fieldDefinition,
            string value,
            bool isCaseSensitive = false)
            where TData : IData
        {
            return filterDefinitionBuilder.Regex(
                field: fieldDefinition,
                regex: CreateBsonRegularExpression(
                    value: value,
                    regexFormat: "${0}$",
                    isCaseSensitive: isCaseSensitive));
        }

        public static FilterDefinition<MongoDocument<TData>> StartsWith<TData>(
            this FilterDefinitionBuilder<MongoDocument<TData>> filterDefinitionBuilder,
            FieldDefinition<MongoDocument<TData>> fieldDefinition,
            string value,
            bool isCaseSensitive = false)
            where TData : IData
        {
            return filterDefinitionBuilder.Regex(
                field: fieldDefinition,
                regex: CreateBsonRegularExpression(
                    value: value,
                    regexFormat: "^{0}",
                    isCaseSensitive: isCaseSensitive));
        }
    }
}

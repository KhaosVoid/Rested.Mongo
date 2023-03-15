using MongoDB.Driver;
using System.Runtime.Serialization;

namespace Rested.Mongo.Data
{
    public class DocumentNotFoundException : MongoException
    {
        #region Constants

        public const string IdPropertyKey = "Id";
        private const string DOCUMENT_NOT_FOUND_EXCEPTION_MESSAGE = "The document with id '{0}' was not found.";

        #endregion Constants

        #region Properties

        public string Code { get; }
        public IDictionary<string, string> Keys { get; private set; } = new Dictionary<string, string>();

        #endregion Properties

        #region Ctor

        public DocumentNotFoundException(string resourceName, Guid id, string? code = null) :
            base(string.Format(DOCUMENT_NOT_FOUND_EXCEPTION_MESSAGE, id))
        {
            Keys.Add(nameof(IdPropertyKey), id.ToString());

            if (!string.IsNullOrWhiteSpace(code))
                Code = code;

            else
                Code = $"{404}";
        }

        #endregion Ctor

        #region Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Code", Code);

            if (Keys.ContainsKey(IdPropertyKey))
                info.AddValue(IdPropertyKey, Keys[IdPropertyKey]);

            else
                info.AddValue(IdPropertyKey, null);
        }

        #endregion Methods
    }
}

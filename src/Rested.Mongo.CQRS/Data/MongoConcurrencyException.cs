using MongoDB.Driver;
using System.Runtime.Serialization;

namespace Rested.Mongo.CQRS.Data
{
    public class MongoConcurrencyException : MongoException
    {
        #region Constants

        public const string IdPropertyKey = "Id";
        private const string CONCURRENCY_EXCEPTION_MESSAGE = "A newer version of the current document exists.";

        #endregion Constants

        #region Properties

        public string Code { get; }
        public IDictionary<string, string> Keys { get; private set; } = new Dictionary<string, string>();

        #endregion Properties

        #region Ctor

        public MongoConcurrencyException(Guid id, string? code = null) : base(CONCURRENCY_EXCEPTION_MESSAGE)
        {
            Keys.Add(nameof(IdPropertyKey), id.ToString());

            if (!string.IsNullOrWhiteSpace(code))
                Code = code;

            else
                Code = $"{409}";
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

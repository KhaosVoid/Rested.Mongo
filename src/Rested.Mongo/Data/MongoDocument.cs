using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Rested.Core.Data;
using System.Text.Json.Serialization;

namespace Rested.Mongo.Data
{
    public class MongoDocument<TData> : IDocument<TData> where TData : IData
    {
        #region Properties

        [BsonId(IdGenerator = typeof(GuidGenerator))]
        public Guid Id { get; set; }

        [BsonIgnore]
        public byte[] ETag
        {
            get => _eTag;
            set
            {
                _eTag = value ?? BitConverter.GetBytes(0UL);
                _updateVersion = BitConverter.ToUInt64(_eTag, 0);
            }
        }

        public DateTime CreateDateTime { get; set; }
        public string CreateUser { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public string UpdateUser { get; set; }

        [JsonIgnore]
        public ulong UpdateVersion
        {
            get => _updateVersion;
            set
            {
                if (_updateVersion != value)
                {
                    _updateVersion = value;
                    _eTag = BitConverter.GetBytes(value);
                }
            }
        }

        public TData Data { get; set; }

        #endregion Properties

        #region Members

        private byte[] _eTag;
        private ulong _updateVersion;

        #endregion Members

        #region Ctor

        public MongoDocument()
        {
            UpdateVersion = 0UL;
            ETag = BitConverter.GetBytes(0UL);
        }

        #endregion Ctor

        #region Methods

        public static MongoDocument<TData> FromDto(Dto<TData> dto)
        {
            return new MongoDocument<TData>()
            {
                Id = dto.Id,
                ETag = dto.ETag,
                Data = dto.Data
            };
        }

        #endregion Methods
    }
}

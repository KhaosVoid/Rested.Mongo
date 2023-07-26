using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Rested.Core.Data;

namespace Rested.Mongo.Server.Serialization.Conventions
{
    public class RestedCamelCaseElementNameConvention : ConventionBase, IMemberMapConvention, IConvention
    {
        #region Methods

        public void Apply(BsonMemberMap memberMap) =>
            memberMap.SetElementName(memberMap.MemberName.ToCamelCase());

        #endregion Methods
    }
}

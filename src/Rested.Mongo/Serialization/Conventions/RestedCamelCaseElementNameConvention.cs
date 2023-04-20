using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Rested.Core.CQRS.Data;

namespace Rested.Mongo.Serialization.Conventions
{
    public class RestedCamelCaseElementNameConvention : ConventionBase, IMemberMapConvention, IConvention
    {
        #region Methods

        public void Apply(BsonMemberMap memberMap) =>
            memberMap.SetElementName(memberMap.MemberName.ToCamelCase());

        #endregion Methods
    }
}

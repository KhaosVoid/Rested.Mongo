using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Rested.Core.Http;
using Rested.Core.MediatR;
using Rested.Core.Validation;
using Rested.Mongo.Data;
using Rested.Mongo.Serialization.Conventions;

namespace Rested.Mongo
{
    public static class Extensions
    {
        public static IServiceCollection AddMongoRested(this IServiceCollection services, bool addMediatR = true, bool addFluentValidation = true, bool addControllers = true)
        {
            services
                .AddSingleton<IMongoClient>(new MongoClient("mongodb://localhost:27017")) //TODO: Move to config
                .AddTransient(sp => sp.GetService<IMongoClient>().GetDatabase("apiSample")) //TODO: Move to config
                .AddTransient<IMongoDocumentAuditingService, MongoDocumentAuditingService>()
                .AddTransient<IMongoSessionFactory, MongoSessionFactory>()
                .AddTransient<ICollectionNameService, CollectionNameService>()
                .AddTransient(typeof(IMongoRepository<>), typeof(MongoRepository<>))
                .AddTransient<IMongoRepositoryFactory, MongoRepositoryFactory>()
                .AddTransient<IMongoContext, MongoContext>();

            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new NullableSerializer<Guid>(new GuidSerializer(BsonType.String)));

            ConventionRegistry.Register("rested", new ConventionPack()
            {
                new RestedCamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            }, _ => true);

            if (addMediatR)
                services.AddMediatRRested();

            if (addFluentValidation)
                services.AddFluentValidationRested();

            if (addControllers)
                services.AddControllersRested();

            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Rested.Core.MediatR;
using Rested.Core.Server.Http;
using Rested.Core.Server.Validation;
using Rested.Mongo.Data;
using Rested.Mongo.Server.Serialization.Conventions;

namespace Rested.Mongo.Server
{
    public static class Extensions
    {
        public static IServiceCollection AddMongoRested(
            this IServiceCollection services,
            MongoClientSettings mongoClientSettings,
            string databaseName,
            bool addMediatR = true,
            bool addFluentValidation = true,
            bool addControllers = true)
        {
            services
                .AddSingleton<IMongoClient>(new MongoClient(mongoClientSettings))
                .AddTransient(sp => sp.GetService<IMongoClient>().GetDatabase(databaseName))
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

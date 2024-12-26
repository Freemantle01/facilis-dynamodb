using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Amazon.DynamoDBv2;

using FacilisDynamoDb.Clients;

using FacilisDynamodb.Entities;

using FacilisDynamoDb.Extensions.DependencyInjection.Credentials;

using FacilisDynamodb.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacilisDynamoDb.Extensions.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTableOptions(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.Configure<TableOptions>(configuration.GetSection(TableOptions.SectionName));
            
            return services;
        }

        public static IServiceCollection AddLocalAmazonDynamoDbClient(
            this IServiceCollection services,
            string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(serviceUrl));
            }

            services.AddScoped<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(
                new FakeAwsCredentials(),
                new AmazonDynamoDBConfig
                {
                    ServiceURL = serviceUrl
                }));
            
            return services;
        }

        public static IServiceCollection AddFacilisDynamoDbClient<TEntity>(
            this IServiceCollection services,
            IJsonTypeInfoResolver jsonTypeInfoResolver) where TEntity : class, IIdentity
        {
            JsonSerializerOptions jsonSerializerOptions = jsonTypeInfoResolver is null
                ? JsonSerializerOptions.Default
                : new JsonSerializerOptions { TypeInfoResolver = jsonTypeInfoResolver };

            services.AddFacilisDynamoDbClient<TEntity>(jsonSerializerOptions);

            return services;
        }
        
        public static IServiceCollection AddFacilisDynamoDbClient<TEntity>(
            this IServiceCollection services,
            JsonSerializerOptions jsonSerializerOptions) where TEntity : class, IIdentity
        { 
            services.AddScoped<IFacilisDynamoDbClient<TEntity>>(svc =>
                new FacilisDynamoDbClient<TEntity>(
                    svc.GetRequiredService<IAmazonDynamoDB>(),
                    svc.GetRequiredService<IOptions<TableOptions>>(),
                    jsonSerializerOptions ?? JsonSerializerOptions.Default,
                    svc.GetRequiredService<ILogger<FacilisDynamoDbClient<TEntity>>>()));

            return services;
        }
    }
}
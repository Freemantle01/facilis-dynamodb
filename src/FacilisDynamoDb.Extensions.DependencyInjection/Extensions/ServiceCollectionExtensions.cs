using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Amazon.DynamoDBv2;

using FacilisDynamodb.Entities;

using FacilisDynamoDb.Extensions.DependencyInjection.Credentials;

using FacilisDynamodb.Options;
using FacilisDynamodb.Repositories;

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

        public static IServiceCollection AddLocalAmazonDynamoDb(
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

        public static IServiceCollection AddFacilisDynamoDb<TEntity>(
            this IServiceCollection services,
            IJsonTypeInfoResolver jsonTypeInfoResolver) where TEntity : class, IIdentity
        {
            JsonSerializerOptions jsonSerializerOptions = jsonTypeInfoResolver is null
                ? JsonSerializerOptions.Default
                : new JsonSerializerOptions { TypeInfoResolver = jsonTypeInfoResolver };

            services.AddFacilisDynamoDb<TEntity>(jsonSerializerOptions);

            return services;
        }
        
        public static IServiceCollection AddFacilisDynamoDb<TEntity>(
            this IServiceCollection services,
            JsonSerializerOptions jsonSerializerOptions) where TEntity : class, IIdentity
        { 
            services.AddScoped<IFacilisDynamoDb<TEntity>>(svc =>
                new FacilisDynamoDb<TEntity>(
                    svc.GetRequiredService<IAmazonDynamoDB>(),
                    svc.GetRequiredService<IOptions<TableOptions>>(),
                    jsonSerializerOptions ?? JsonSerializerOptions.Default,
                    svc.GetRequiredService<ILogger<FacilisDynamoDb<TEntity>>>()));

            return services;
        }
    }
}
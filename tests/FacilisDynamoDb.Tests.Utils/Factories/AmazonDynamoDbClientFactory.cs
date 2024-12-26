using Amazon.DynamoDBv2;

using FacilisDynamoDb.Extensions.DependencyInjection.Credentials;
using FacilisDynamoDb.Tests.Utils.Constants;

namespace FacilisDynamoDb.Tests.Utils.Factories
{
    public static class AmazonDynamoDbClientFactory
    {
        public static AmazonDynamoDBClient CreateClient()
        {
            var amazonDynamoDbConfig = new AmazonDynamoDBConfig()
            {
                ServiceURL = AmazonDynamoDbConstants.ServiceUrl
            };

            return new AmazonDynamoDBClient(new FakeAwsCredentials(), amazonDynamoDbConfig);
        }
    }
}
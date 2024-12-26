using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using FacilisDynamoDb.Tests.Utils.Constants;

using Xunit;

namespace FacilisDynamoDb.Tests.Utils.Fixtures
{
    public class AmazonDynamoDbFixture : IAsyncLifetime
    {
        private readonly IContainer _dynamoDatabaseContainer =
            new ContainerBuilder()
                .WithImage("amazon/dynamodb-local:latest")
                .WithPortBinding(AmazonDynamoDbConstants.Port, AmazonDynamoDbConstants.Port)
                .WithCommand("-jar", "DynamoDBLocal.jar", "-sharedDb", "true") 
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(AmazonDynamoDbConstants.Port))
                .Build();

    
        public async Task InitializeAsync()
        {
            await _dynamoDatabaseContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _dynamoDatabaseContainer.StopAsync();
        }
    }
}
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using FacilisDynamoDb.Generators;
using FacilisDynamoDb.Tests.Utils.Constants;
using FacilisDynamoDb.Tests.Utils.Factories;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FacilisDynamoDb.Tests.Utils.Bases
{
    public class AmazonDynamoDbTestBase : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            using (AmazonDynamoDBClient client = AmazonDynamoDbClientFactory.CreateClient())
            {
                using (var tableGenerator = new TableGenerator(
                           client,
                           TableOptionsFactory.Create(),
                           new Mock<ILogger<TableGenerator>>().Object))
                {
                    await tableGenerator.CreateTableAsync();
                }
            }
        }

        public async Task DisposeAsync()
        {
            using (AmazonDynamoDBClient client = AmazonDynamoDbClientFactory.CreateClient())
            {
                var deleteTableRequest = new DeleteTableRequest()
                {
                    TableName = AmazonDynamoDbConstants.TableName
                };

                _ = await client.DeleteTableAsync(deleteTableRequest);
            }
        }
    }
}
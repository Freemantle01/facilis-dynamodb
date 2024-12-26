using System.Threading.Tasks;

using Amazon.DynamoDBv2;

using FacilisDynamoDb.Generators;
using FacilisDynamoDb.Tests.Utils.Constants;
using FacilisDynamoDb.Tests.Utils.Factories;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FacilisDynamoDb.Tests.Utils.Bases
{
    public class AmazonDynamoDbTableCleanerBase : IAsyncLifetime
    {
        private readonly string _tableName;

        public AmazonDynamoDbTableCleanerBase(string tableName = AmazonDynamoDbConstants.TableName)
        {
            _tableName = tableName;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using (AmazonDynamoDBClient client = AmazonDynamoDbClientFactory.CreateClient())
            {
                using (var tableGenerator = new TableGenerator(
                           client,
                           TableOptionsFactory.Create(_tableName),
                           new Mock<ILogger<TableGenerator>>().Object))
                {
                    await tableGenerator.CreateTableAsync();
                }
            }
        }
    }
}
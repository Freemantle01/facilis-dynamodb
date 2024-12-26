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
    public class AmazonDynamoDbTestBase : IAsyncLifetime
    {
        private readonly string _tableName;

        public AmazonDynamoDbTestBase(string tableName = AmazonDynamoDbConstants.TableName)
        {
            _tableName = tableName;
        }
        
        public async Task InitializeAsync()
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

        public async Task DisposeAsync()
        {
            var cleaner = new AmazonDynamoDbTableCleanerBase(_tableName);
            await cleaner.DisposeAsync();
        }
    }
}
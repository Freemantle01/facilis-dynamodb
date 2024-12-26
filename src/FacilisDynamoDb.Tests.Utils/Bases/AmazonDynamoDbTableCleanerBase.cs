using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using FacilisDynamoDb.Tests.Utils.Constants;
using FacilisDynamoDb.Tests.Utils.Factories;

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
                var deleteTableRequest = new DeleteTableRequest()
                {
                    TableName = _tableName
                };

                _ = await client.DeleteTableAsync(deleteTableRequest);
            }
        }
    }
}
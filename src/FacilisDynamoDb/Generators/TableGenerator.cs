using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using FacilisDynamodb.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacilisDynamoDb.Generators
{
    public class TableGenerator : ITableGenerator
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly IOptions<TableOptions> _options;
        private readonly ILogger<TableGenerator> _logger;

        public TableGenerator(IAmazonDynamoDB amazonDynamoDb, 
            IOptions<TableOptions> options,
            ILogger<TableGenerator> logger)
        {
            _amazonDynamoDb = amazonDynamoDb;
            _options = options;
            _logger = logger;
        }

        public async Task CreateTableAsync()
        {
            var request = new CreateTableRequest()
            {
                TableName = _options.Value.Name,
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        AttributeName = "pk",
                        AttributeType = "S"
                    },
                    new AttributeDefinition()
                    {
                        AttributeName = "sk",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>()
                {
                    new KeySchemaElement()
                    {
                        AttributeName = "pk",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement()
                    {
                        AttributeName = "sk",
                        KeyType = "RANGE"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 25,
                    WriteCapacityUnits = 25
                }
            };
        
            try
            {
                CreateTableResponse response = await _amazonDynamoDb.CreateTableAsync(request);
        
                _logger.LogInformation("Created Amazon DynamoDb table: {Name}, status code: {StatusCode}",
                    _options.Value.Name, response.HttpStatusCode);
            }
            catch (ResourceInUseException)
            {
                _logger.LogInformation("Amazon DynamoDb table: {Name} already exists...", _options.Value.Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _amazonDynamoDb.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
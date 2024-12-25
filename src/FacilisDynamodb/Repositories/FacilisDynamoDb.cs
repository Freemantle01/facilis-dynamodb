using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

using FacilisDynamodb.Constants;
using FacilisDynamodb.Entities;
using FacilisDynamodb.Exceptions;
using FacilisDynamodb.Extensions;
using FacilisDynamodb.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Identity = FacilisDynamodb.Entities.Identity;

namespace FacilisDynamodb.Repositories
{
    public class FacilisDynamoDb<TEntity>: IFacilisDynamoDb<TEntity> 
        where TEntity: class, IIdentity
    {
        private readonly string _entityExistsCondition = 
            $"attribute_exists({TableConstants.PrimaryKeyName}) AND attribute_exists({TableConstants.SortKeyName})";
        private const int BatchLimit = 25;
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly IOptions<TableOptions> _tableOptions;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogger<FacilisDynamoDb<TEntity>> _logger;
        
        public FacilisDynamoDb(
            IAmazonDynamoDB amazonDynamoDb, 
            IOptions<TableOptions> tableOptions,
            JsonSerializerOptions jsonSerializerOptions,
            ILogger<FacilisDynamoDb<TEntity>> logger)
        {
            _amazonDynamoDb = amazonDynamoDb ?? throw new ArgumentNullException(nameof(amazonDynamoDb));
            _tableOptions = tableOptions ?? throw new ArgumentNullException(nameof(tableOptions));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    
        public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
        
            var createItemRequest = new PutItemRequest()
            {
                TableName = _tableOptions.Value.Name,
                Item = Document.FromJson(JsonSerializer.Serialize(entity, _jsonSerializerOptions)).ToAttributeMap(),
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            PutItemResponse putItemResponse = await _amazonDynamoDb.PutItemAsync(createItemRequest,
                cancellationToken);
        
            _logger.LogInformation("Entity created for pk: {Pk} sk: {Sk}", 
                entity.PrimaryKey, 
                entity.SortKey);
            _logger.LogConsumedCapacity(putItemResponse.ConsumedCapacity, "CreateEntity");
        }
    
        public async Task<TEntity> GetAsync(IIdentity identity, CancellationToken cancellationToken = default)
        {
            var getItemRequest = new GetItemRequest()
            {
                TableName = _tableOptions.Value.Name,
                Key = new Dictionary<string, AttributeValue>
                {
                    { TableConstants.PrimaryKeyName, new AttributeValue { S = identity.PrimaryKey } },
                    { TableConstants.SortKeyName, new AttributeValue { S = identity.SortKey } }
                },
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            GetItemResponse response = await _amazonDynamoDb.GetItemAsync(getItemRequest,
                cancellationToken);
        
            _logger.LogConsumedCapacity(response.ConsumedCapacity, "GetEntity");
        
            if (response.Item.Count == 0)
            {
                return null;
            }

            Document itemAsDocument = Document.FromAttributeMap(response.Item);

            TEntity entity = JsonSerializer.Deserialize<TEntity>(itemAsDocument.ToJson(), _jsonSerializerOptions);
        
            _logger.LogInformation("Get entity pk: {Pk} sk: {Sk}, found: {Found}",
                identity.PrimaryKey, 
                identity.SortKey, 
                entity != null);
        
            return entity;
        }
    
        public async Task<IEnumerable<TEntity>> GetAllAsync(string primaryKey, CancellationToken cancellationToken = default)
        {
            // https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/LowLevelDotNetQuerying.html
        
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;
            var output = new List<TEntity>();
            do
            {
                var queryRequest = new QueryRequest()
                {
                    TableName = _tableOptions.Value.Name,
                    KeyConditionExpression = "#pk = :primaryKeyValue",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#pk", TableConstants.PrimaryKeyName }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":primaryKeyValue", new AttributeValue { S = primaryKey } }
                    },
                    ExclusiveStartKey = lastKeyEvaluated,
                    ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL 
                };
            
                QueryResponse response = await _amazonDynamoDb.QueryAsync(queryRequest,
                    cancellationToken);

                _logger.LogConsumedCapacity(response.ConsumedCapacity, "GetAllEntities");

                output.AddRange(response.Items.Select(item 
                    => JsonSerializer.Deserialize<TEntity>(Document.FromAttributeMap(item).ToJson(), _jsonSerializerOptions)));

                lastKeyEvaluated = response.LastEvaluatedKey;
            
            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

            return output;
        }
    
        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var createItemRequest = new PutItemRequest()
            {
                TableName = _tableOptions.Value.Name,
                Item = Document.FromJson(JsonSerializer.Serialize(entity, _jsonSerializerOptions)).ToAttributeMap(),
                ConditionExpression = _entityExistsCondition,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            try
            {
                PutItemResponse putItemResponse = await _amazonDynamoDb.PutItemAsync(
                    createItemRequest,
                    cancellationToken);
            
                _logger.LogInformation("Entity updated pk: {Pk} sk: {Sk}", 
                    entity.PrimaryKey, 
                    entity.SortKey);
                _logger.LogConsumedCapacity(putItemResponse.ConsumedCapacity, "UpdateEntity");
            }
            catch (ConditionalCheckFailedException e)
            {
                HandleEntityNotFound(entity, e);
            }
        }

        public async Task DeleteAsync(IIdentity identity, CancellationToken cancellationToken = default)
        {
            var deleteItemRequest = new DeleteItemRequest()
            {
                TableName = _tableOptions.Value.Name,
                Key = new Dictionary<string, AttributeValue>
                {
                    { TableConstants.PrimaryKeyName, new AttributeValue { S = identity.PrimaryKey } },
                    { TableConstants.SortKeyName, new AttributeValue { S = identity.SortKey } }
                },
                ConditionExpression = _entityExistsCondition,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            try
            {
                DeleteItemResponse deleteItemResponse = await _amazonDynamoDb.DeleteItemAsync(
                    deleteItemRequest,
                    cancellationToken);
                _logger.LogInformation("Entity deleted pk: {Pk} sk: {Sk}", 
                    identity.PrimaryKey, 
                    identity.SortKey);
                _logger.LogConsumedCapacity(deleteItemResponse.ConsumedCapacity, "DeleteEntity");
            }
            catch (ConditionalCheckFailedException e)
            {
                HandleEntityNotFound(identity, e);
            }
        }

        public async Task DeleteAllAsync(string primaryKey, CancellationToken cancellationToken = default)
        {
            List<Identity> identities = await GetAllIdentitiesAsync(primaryKey,
                cancellationToken);
        
            int skipCount = 0;
            while (skipCount < identities.Count)
            {
                await BatchDeleteAsync(identities.Skip(skipCount)
                        .Take(BatchLimit)
                        .ToList(), 
                    cancellationToken);

                skipCount += BatchLimit;
            }
        
            _logger.LogInformation("Deleted all items for primary key: {PrimaryKey}, count: {ItemsCount}", 
                primaryKey, identities.Count);
        }

        private async Task BatchDeleteAsync(
            IReadOnlyCollection<Identity> identities, 
            CancellationToken cancellationToken)
        {
            if (identities.Count > BatchLimit)
            {
                throw new ArgumentException($"Identities count is greater than batch limit, " +
                    $"identities count: {identities.Count}, batch limit: {BatchLimit}");
            }

            List<WriteRequest> writeRequests = identities
                .Select(identity => new WriteRequest
                {
                    DeleteRequest = new DeleteRequest()
                    {
                        Key = Document.FromJson(JsonSerializer.Serialize(identity, _jsonSerializerOptions))
                            .ToAttributeMap()
                    }
                }).ToList();

            var batchWriteItemRequest = new BatchWriteItemRequest()
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>()
                {
                    {
                        _tableOptions.Value.Name,
                        writeRequests
                    }
                },
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };
        
            try
            {
                BatchWriteItemResponse response =  await _amazonDynamoDb.BatchWriteItemAsync(
                    batchWriteItemRequest, 
                    cancellationToken);

                _logger.LogConsumedCapacity(response.ConsumedCapacity);
            
                if (response.UnprocessedItems.Count > 0)
                {
                    _logger.LogError("Some items were not processed (deleted):");
                    foreach (KeyValuePair<string, List<WriteRequest>> item in response.UnprocessedItems)
                    {
                        _logger.LogError("Table: {TableName}, Count: {ItemsCount}", 
                            item.Key, item.Value.Count);
                    }
                
                    throw new BatchWriteException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error performing batch delete: {ErrorMessage}", e.Message);
                throw;
            }
        }

        private async Task<List<Identity>> GetAllIdentitiesAsync(string primaryKey, CancellationToken cancellationToken)
        {
            var identities = new List<Identity>();
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;
        
            do
            {
                var queryRequest = new QueryRequest()
                {
                    TableName = _tableOptions.Value.Name,
                    KeyConditionExpression = "#pk = :primaryKeyValue",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        { "#pk", TableConstants.PrimaryKeyName }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        {
                            ":primaryKeyValue", 
                            new AttributeValue()
                            {
                                S = primaryKey
                            }
                        }
                    },
                    ProjectionExpression = $"{TableConstants.PrimaryKeyName}, {TableConstants.SortKeyName}",
                    ExclusiveStartKey = lastKeyEvaluated,
                    ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
                };
            
                QueryResponse response = await _amazonDynamoDb.QueryAsync(queryRequest,
                    cancellationToken);

                _logger.LogConsumedCapacity(response.ConsumedCapacity, "GetAllIdentities");

                identities.AddRange(response.Items.Select(item 
                    => JsonSerializer.Deserialize<Identity>(Document.FromAttributeMap(item).ToJson(), _jsonSerializerOptions)));

                lastKeyEvaluated = response.LastEvaluatedKey;
            
            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

            return identities;
        }
    
        private void HandleEntityNotFound(IIdentity identity, Exception e)
        {
            _logger.LogError(e,
                "Entity not found with pk: {Pk} sk: {Sk}",
                identity.PrimaryKey,
                identity.SortKey);
            throw new EntityNotFoundException(identity);
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
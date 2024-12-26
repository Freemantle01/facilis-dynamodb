using System.Collections.Generic;

using Amazon.DynamoDBv2.Model;

using Microsoft.Extensions.Logging;

namespace FacilisDynamodb.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogConsumedCapacity<T>(this ILogger<T> logger, 
            ConsumedCapacity consumedCapacity, 
            string operationName = null)
        {
            if (consumedCapacity is null)
            {
                return;
            }
        
            logger.LogInformation("Consumed capacity, " +
                "total: {Total}, write: {Write}, read: {Read}, " +
                "table name: {TableName}, operation name: {OperationName}",
                consumedCapacity.CapacityUnits,
                consumedCapacity.WriteCapacityUnits,
                consumedCapacity.ReadCapacityUnits,
                consumedCapacity.TableName,
                operationName);
        }

        public static void LogConsumedCapacity<T>(this ILogger<T> logger, 
            List<ConsumedCapacity> consumedCapacities,
            string operationName = null)
        {
            foreach (ConsumedCapacity consumedCapacity in consumedCapacities)
            {
                logger.LogConsumedCapacity(consumedCapacity, operationName);
            }
        }
    }
}
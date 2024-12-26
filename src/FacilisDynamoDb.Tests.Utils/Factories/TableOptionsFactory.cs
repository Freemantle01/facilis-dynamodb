using FacilisDynamodb.Options;

using FacilisDynamoDb.Tests.Utils.Constants;

using Microsoft.Extensions.Options;

namespace FacilisDynamoDb.Tests.Utils.Factories
{
    public static class TableOptionsFactory
    {
        public static IOptions<TableOptions> Create()
        {
            return Options.Create(new TableOptions
            {
                Name = AmazonDynamoDbConstants.TableName
            });
        }
    }
}
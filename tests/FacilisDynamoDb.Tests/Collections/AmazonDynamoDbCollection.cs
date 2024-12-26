using FacilisDynamoDb.Tests.Utils.Constants;
using FacilisDynamoDb.Tests.Utils.Fixtures;

namespace FacilisDynamoDb.Tests.Collections;

[CollectionDefinition(CollectionFixtureInfo.AmazonDynamoDatabaseCollectionName)]
public class AmazonDynamoDatabaseCollection : ICollectionFixture<AmazonDynamoDbFixture>;
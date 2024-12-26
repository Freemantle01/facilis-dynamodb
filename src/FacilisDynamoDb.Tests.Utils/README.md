# FacilisDynamoDb.Tests.Utils

## Overview

FacilisDynamoDb.Tests.Utils provides utility classes and constants to facilitate testing for the FacilisDynamoDb project. It includes fixtures, constants, and other helper classes to streamline the setup and execution of tests.

## Features

- Utility classes for testing
- Constants for test configurations
- Fixtures for setting up test environments
- `AmazonDynamoDbFixture` for managing DynamoDB setup and teardown
- `AmazonDynamoDbTestBase` for base test class with common DynamoDB test functionality

## Usage
To use `AmazonDynamoDbFixture` you need to create in your test assembly collection
```csharp
[CollectionDefinition(CollectionFixtureInfo.AmazonDynamoDatabaseCollectionName)]
public class AmazonDynamoDatabaseCollection : ICollectionFixture<AmazonDynamoDbFixture>;
```
and mark your test class
```csharp
[Collection(CollectionFixtureInfo.AmazonDynamoDatabaseCollectionName)]
public sealed class FacilisDynamoDbClientTests : AmazonDynamoDbTestBase
{
}
```

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
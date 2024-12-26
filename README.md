# FacilisDynamoDb

## Overview

FacilisDynamoDb is a simple AWS DynamoDB repository for .NET applications. It provides a straightforward way to interact with DynamoDB, including basic CRUD operations and table management.

## Features

- Easy integration with AWS DynamoDB
- Basic CRUD operations
- Table management
- Supports .NET Standard 2.0

## Installation

To install the package, add the following `PackageReference` to your `.csproj` file:

```xml
<PackageReference Include="FacilisDynamoDb" Version="1.0.0" />
```

# FacilisDynamoDb.Extensions.DependencyInjection

## Overview

FacilisDynamoDb.Extensions.DependencyInjection provides dependency injection extensions for .NET applications, simplifying the integration of AWS DynamoDB.

## Features

- Easy integration with AWS DynamoDB
- Dependency injection support
- Simple configuration
- Supports .NET Standard 2.0

## Installation

To install the package, add the following `PackageReference` to your `.csproj` file:

```xml
<PackageReference Include="FacilisDynamoDb.Extensions.DependencyInjection" Version="1.0.0" />
```

# FacilisDynamoDb.Tests.Utils

## Overview

FacilisDynamoDb.Tests.Utils provides utility classes and constants to facilitate testing for the FacilisDynamoDb project. It includes fixtures, constants, and other helper classes to streamline the setup and execution of tests.

## Features

- Utility classes for testing
- Constants for test configurations
- Fixtures for setting up test environments
- `AmazonDynamoDbFixture` for managing DynamoDB setup and teardown
- `AmazonDynamoDbTestBase` for base test class with common DynamoDB test functionality

## Installation

To include this project in your solution, add a project reference in your `.csproj` file:

```xml
<ProjectReference Include="..\FacilisDynamoDb.Tests.Utils\FacilisDynamoDb.Tests.Utils.csproj" />
```

## License

All projects are licensed under the MIT License. See the `LICENSE` file for more details.
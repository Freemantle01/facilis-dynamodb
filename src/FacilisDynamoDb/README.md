# FacilisDynamoDb

## Overview

FacilisDynamoDb is a simple AWS DynamoDB client for .NET applications. It provides a straightforward way to interact with DynamoDB, including basic CRUD operations and table management.

## Features

- Easy integration with AWS DynamoDB
- Basic CRUD operations
- Table management
- Supports .NET Standard 2.0

## Usage

### Configuration

1. Add the necessary services to the `WebApplicationBuilder` in your `Program.cs`:

```csharp
using FacilisDynamoDb.Extensions.DependencyInjection.Extensions;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddFacilisDynamoDbClient<TodoEntity>(AppJsonSerializerContext.Default);
builder.Services.AddTableOptions(builder.Configuration);
builder.Services.AddLocalAmazonDynamoDbClient(builder.Configuration.GetValue<string>("AmazonDynamoDbServiceUrl")!);
builder.Services.AddScoped<TableGenerator>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

WebApplication app = builder.Build();
```

2. Define your DynamoDB entity:

```csharp
public class TodoEntity : IIdentity
{
    [JsonPropertyName(TableConstants.PrimaryKeyName)]
    public string PrimaryKey { get; set; } = default!;

    [JsonPropertyName(TableConstants.SortKeyName)]
    public string SortKey { get; set; } = default!;

    public int Id { get; set; }
    public string? Title { get; set; }
    public DateOnly? DueBy { get; set; }
    public bool IsComplete { get; set; }
}
```

3. Create and populate the DynamoDB table:

```csharp
async Task CreateAmazonDynamoDbTableAsync(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    using TableGenerator tableGenerator = scope.ServiceProvider.GetRequiredService<TableGenerator>();

    await tableGenerator.CreateTableAsync();
}

async Task PopulateDatabaseAsync(WebApplication app)
{
    var sampleTodos = new TodoEntity[]
    {
        new() { Id = 1, PrimaryKey = "Note", SortKey = "1", Title = "Walk the dog", IsComplete = true },
        new() { Id = 2, PrimaryKey = "Note", SortKey = "2", Title = "Do the dishes", DueBy = DateOnly.FromDateTime(DateTime.Now), IsComplete = true },
        new() { Id = 3, PrimaryKey = "Note", SortKey = "3", Title = "Do the laundry", DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), IsComplete = false }
    };

    using IServiceScope scope = app.Services.CreateScope();
    using IFacilisDynamoDbClient<TodoEntity> client = scope.ServiceProvider.GetRequiredService<IFacilisDynamoDbClient<TodoEntity>>();

    List<TodoEntity> notes = (await client.GetAllAsync("Note")).ToList();
    if (notes.Count != 0) return;

    foreach (TodoEntity sampleTodo in sampleTodos)
    {
        await client.CreateAsync(sampleTodo);
    }
}
```

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
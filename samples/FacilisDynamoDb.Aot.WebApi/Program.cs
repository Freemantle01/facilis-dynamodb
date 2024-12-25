using System.Text.Json;
using System.Text.Json.Serialization;

using Amazon.DynamoDBv2;

using FacilisDynamodb.Constants;

using FacilisDynamoDb.Credentials;

using FacilisDynamodb.Entities;

using FacilisDynamoDb.Generators;

using FacilisDynamodb.Options;
using FacilisDynamodb.Repositories;

using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddScoped<IFacilisDynamoDb<TodoEntity>>(svc => 
    new FacilisDynamoDb<TodoEntity>(
        svc.GetRequiredService<IAmazonDynamoDB>(),
        svc.GetRequiredService<IOptions<TableOptions>>(),
        new JsonSerializerOptions
        {
            TypeInfoResolver = AppJsonSerializerContext.Default
        },
        svc.GetRequiredService<ILogger<FacilisDynamoDb<TodoEntity>>>()));
builder.Services.Configure<TableOptions>(builder.Configuration.GetSection(TableOptions.SectionName));
builder.Services.AddScoped<TableGenerator>();
builder.Services.AddScoped<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(
    new FakeAwsCredentials(),
    new AmazonDynamoDBConfig
    {
        ServiceURL = builder.Configuration.GetValue<string>("AmazonDynamoDbServiceUrl")
    }));
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0,
        AppJsonSerializerContext.Default);
});

WebApplication app = builder.Build();

await CreateAmazonDynamoDbTableAsync(app);

await PopulateDatabaseAsync(app);

var todosApi = app.MapGroup("/todos");
todosApi.MapGet(
    "/",
    async (IFacilisDynamoDb<TodoEntity> facilisDynamoDb) => await facilisDynamoDb.GetAllAsync("Note"));
todosApi.MapGet(
    "/{id:int}",
    async (int id, IFacilisDynamoDb<TodoEntity> facilisDynamoDb) =>
    {
        TodoEntity? todo = await facilisDynamoDb.GetAsync(new Identity
        {
            PrimaryKey = "Note",
            SortKey = id.ToString()
        });
        
        return todo is null ? Results.NotFound() : Results.Ok(todo);
    });
        

await app.RunAsync();

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
        new()
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "Walk the dog",
            DueBy = null,
            IsComplete = true
        },
        new()
        {
            Id = 2,
            PrimaryKey = "Note",
            SortKey = "2",
            Title = "Do the dishes",
            DueBy = DateOnly.FromDateTime(DateTime.Now),
            IsComplete = true,
        },
        new()
        {
            Id = 3,
            PrimaryKey = "Note",
            SortKey = "3",
            Title = "Do the laundry",
            DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            IsComplete = false
        }
    };
    
    using IServiceScope scope = app.Services.CreateScope();
    using IFacilisDynamoDb<TodoEntity> facilisDynamoDb = scope.ServiceProvider.GetRequiredService<IFacilisDynamoDb<TodoEntity>>();

    List<TodoEntity> notes = (await facilisDynamoDb.GetAllAsync("Note")).ToList();
    if (notes.Count != 0)
    {
        return;
    }

    foreach (TodoEntity sampleTodo in sampleTodos)
    {
        await facilisDynamoDb.CreateAsync(sampleTodo);
    }
}

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

[JsonSerializable(typeof(IEnumerable<TodoEntity>))]
[JsonSerializable(typeof(TodoEntity))]
[JsonSerializable(typeof(Identity))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}


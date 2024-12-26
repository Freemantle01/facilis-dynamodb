using System.Text.Json.Serialization;

using FacilisDynamoDb.Clients;

using FacilisDynamodb.Constants;
using FacilisDynamodb.Entities;

using FacilisDynamoDb.Extensions.DependencyInjection.Extensions;
using FacilisDynamoDb.Generators;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddFacilisDynamoDbClient<TodoEntity>(AppJsonSerializerContext.Default);
builder.Services.AddTableOptions(builder.Configuration);
builder.Services.AddLocalAmazonDynamoDbClient(builder.Configuration.GetValue<string>("AmazonDynamoDbServiceUrl")!);
builder.Services.AddScoped<TableGenerator>();
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
    async (IFacilisDynamoDbClient<TodoEntity> facilisDynamoDb) => await facilisDynamoDb.GetAllAsync("Note"));
todosApi.MapGet(
    "/{id:int}",
    async (int id, IFacilisDynamoDbClient<TodoEntity> facilisDynamoDb) =>
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
    using IFacilisDynamoDbClient<TodoEntity> client = scope.ServiceProvider.GetRequiredService<IFacilisDynamoDbClient<TodoEntity>>();

    List<TodoEntity> notes = (await client.GetAllAsync("Note")).ToList();
    if (notes.Count != 0)
    {
        return;
    }

    foreach (TodoEntity sampleTodo in sampleTodos)
    {
        await client.CreateAsync(sampleTodo);
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


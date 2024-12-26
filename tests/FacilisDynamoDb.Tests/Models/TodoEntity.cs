using System.Text.Json.Serialization;

using FacilisDynamodb.Constants;
using FacilisDynamodb.Entities;

namespace FacilisDynamoDb.Tests.Models;

public class TodoEntity : IIdentity
{
    [JsonPropertyName(TableConstants.PrimaryKeyName)]
    public string PrimaryKey { get; set; } = default!;

    [JsonPropertyName(TableConstants.SortKeyName)]
    public string SortKey { get; set; } = default!;

    public int Id { get; set; }
    
    public string? Title { get; set; }
    
    public bool IsComplete { get; set; }
}
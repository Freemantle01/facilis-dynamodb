using System.Text.Json;

using FacilisDynamoDb.Clients;

using FacilisDynamodb.Entities;
using FacilisDynamodb.Exceptions;

using FacilisDynamoDb.Tests.Constants;
using FacilisDynamoDb.Tests.Models;
using FacilisDynamoDb.Tests.Utils.Bases;
using FacilisDynamoDb.Tests.Utils.Constants;
using FacilisDynamoDb.Tests.Utils.Factories;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

namespace FacilisDynamoDb.Tests.Clients;

[Trait(TraitConstants.Category, TraitConstants.IntegrationTest)]
[Collection(CollectionFixtureInfo.AmazonDynamoDatabaseCollectionName)]
public sealed class FacilisDynamoDbClientTests : AmazonDynamoDbTestBase, IDisposable
{
    private readonly FacilisDynamoDbClient<TodoEntity> _client;

    public FacilisDynamoDbClientTests()
    {
        _client = new FacilisDynamoDbClient<TodoEntity>(
            AmazonDynamoDbClientFactory.CreateClient(),
            TableOptionsFactory.Create(),
            JsonSerializerOptions.Default,
            new Mock<ILogger<FacilisDynamoDbClient<TodoEntity>>>().Object);
    }
    
    [Fact]
    public async Task GetAsync_WithMissingEntity_ShouldReturnNull()
    {
        // Arrange
        var identity = new Identity
        {
            PrimaryKey = "somePk",
            SortKey = "someSk"
        };
        
        // Act
        TodoEntity? entity = await _client.GetAsync(identity);

        // Assert
        entity.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAsync_WithInvalidIdentity_ShouldReturnNull()
    {
        // Arrange
        var newTodoEntity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "title",
            IsComplete = true
        };
        var invalidIdentity = new Identity
        {
            PrimaryKey = "invalidPk",
            SortKey = "invalidSk"
        };

        await _client.CreateAsync(newTodoEntity);
        
        // Act
        TodoEntity? entity = await _client.GetAsync(invalidIdentity);

        // Assert
        entity.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAsync_WithValidIdentity_ShouldReturnEntity()
    {
        // Arrange
        var newTodoEntity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "title",
            IsComplete = true
        };

        await _client.CreateAsync(newTodoEntity);
        
        // Act
        TodoEntity? entity = await _client.GetAsync(newTodoEntity);

        // Assert
        entity.Should().BeEquivalentTo(newTodoEntity);
    }
    
    [Fact]
    public async Task GetAllAsync_WithInvalidIdentity_ShouldReturnEmptyCollection()
    {
        // Arrange
        var primaryKey = "somePk";
        
        // Act
        List<TodoEntity>? entities = await _client.GetAllAsync(primaryKey);

        // Assert
        entities.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllAsync_WithManyEntities_ShouldReturnOnlyTwoEntities()
    {
        // Arrange
        var primaryKey = "somePk";
        List<TodoEntity> expectedEntities =
        [
            new TodoEntity
            {
                Id = 1,
                PrimaryKey = primaryKey,
                SortKey = "1",
                Title = "first",
                IsComplete = true
            },
            new TodoEntity
            {
                Id = 2,
                PrimaryKey = primaryKey,
                SortKey = "2",
                Title = "second",
                IsComplete = false
            }
        ];
        List<TodoEntity> allEntities =
        [
            expectedEntities[0],
            expectedEntities[1],
            new TodoEntity
            {
                Id = 3,
                PrimaryKey = "otherPk",
                SortKey = "3",
                Title = "third",
                IsComplete = false
            },
            new TodoEntity
            {
                Id = 4,
                PrimaryKey = "differentPk",
                SortKey = "4",
                Title = "fourth",
                IsComplete = true
            },
        ];
        
        foreach (TodoEntity entity in allEntities)
        {
            await _client.CreateAsync(entity);
        }
        
        // Act
        List<TodoEntity>? actualEntities = await _client.GetAllAsync(primaryKey);

        // Assert
        actualEntities.Should().BeEquivalentTo(expectedEntities);
    } 
    
    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldCreateEntity()
    {
        // Arrange
        var newTodoEntity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "title",
            IsComplete = true
        };
        
        // Act
        await _client.CreateAsync(newTodoEntity);

        // Assert
        List<TodoEntity> todos = await _client.GetAllAsync(newTodoEntity.PrimaryKey);
        todos.Count.Should().Be(1);
        todos.Single().Should().BeEquivalentTo(newTodoEntity);
    }

    [Fact]
    public async Task UpdateAsync_WithMissingEntity_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var entity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "title",
            IsComplete = true
        };
        Func<Task> act = () => _client.UpdateAsync(entity);

        // Act && Assert
        EntityNotFoundException? exception = (await act.Should().ThrowAsync<EntityNotFoundException>()).Subject.Single();
        exception.Message.Should().Be($"Entity not found, primaryKey: {entity.PrimaryKey}, sortKey: {entity.SortKey}");
        exception.PrimaryKey.Should().Be(entity.PrimaryKey);
        exception.SortKey.Should().Be(entity.SortKey);
    }
    
    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateEntity()
    {
        // Arrange
        var newTodoEntity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "title",
            IsComplete = true
        };
        var updatedTodoEntity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "newValue",
            IsComplete = false
        }; 
        await _client.CreateAsync(newTodoEntity);
        
        // Act
        await _client.UpdateAsync(updatedTodoEntity);

        // Assert
        List<TodoEntity> todos = await _client.GetAllAsync(newTodoEntity.PrimaryKey);
        todos.Count.Should().Be(1);
        todos.Single().Should().BeEquivalentTo(updatedTodoEntity);
    }

    [Fact]
    public async Task DeleteAsync_WithMissingEntity_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var identity = new Identity
        {
            PrimaryKey = "pk",
            SortKey = "sk"
        };

        Func<Task> act = () => _client.DeleteAsync(identity);

        // Act && Assert
        EntityNotFoundException? exception = (await act.Should().ThrowAsync<EntityNotFoundException>()).Subject.Single();
        exception.Message.Should().Be($"Entity not found, primaryKey: {identity.PrimaryKey}, sortKey: {identity.SortKey}");
        exception.PrimaryKey.Should().Be(identity.PrimaryKey);
        exception.SortKey.Should().Be(identity.SortKey);
    }
    
    [Fact]
    public async Task DeleteAsync_WithValidIdentity_ShouldRemoveEntity()
    {
        // Arrange
        var newTodoEntity = new TodoEntity
        {
            Id = 1,
            PrimaryKey = "Note",
            SortKey = "1",
            Title = "title",
            IsComplete = true
        };
        await _client.CreateAsync(newTodoEntity);
        
        // Act
        await _client.DeleteAsync(newTodoEntity);
        
        // Assert
        List<TodoEntity> todos = await _client.GetAllAsync(newTodoEntity.PrimaryKey);
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllAsync_WithValidPrimaryKey_ShouldRemoveAll()
    {
        // Arrange
        var primaryKey = "somePk";
        List<TodoEntity> expectedEntities =
        [
            new TodoEntity
            {
                Id = 1,
                PrimaryKey = primaryKey,
                SortKey = "1",
                Title = "first",
                IsComplete = true
            },
            new TodoEntity
            {
                Id = 2,
                PrimaryKey = primaryKey,
                SortKey = "2",
                Title = "second",
                IsComplete = false
            }
        ];
        
        foreach (TodoEntity entity in expectedEntities)
        {
            await _client.CreateAsync(entity);
        }
        
        // Act
        await _client.DeleteAllAsync(primaryKey);
        
        // Assert
        List<TodoEntity>? actualEntities = await _client.GetAllAsync(primaryKey);
        actualEntities.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllAsync_WithManyEntities_ShouldRemoveTwoAll()
    {
        // Arrange
        var primaryKey = "somePk";
        var otherPrimaryKey = "otherPk";
        List<TodoEntity> expectedEntities =
        [
            new TodoEntity
            {
                Id = 1,
                PrimaryKey = primaryKey,
                SortKey = "1",
                Title = "first",
                IsComplete = true
            },
            new TodoEntity
            {
                Id = 2,
                PrimaryKey = primaryKey,
                SortKey = "2",
                Title = "second",
                IsComplete = false
            }
        ];
        List<TodoEntity> otherEntities =
        [
            new TodoEntity
            {
                Id = 3,
                PrimaryKey = otherPrimaryKey,
                SortKey = "3",
                Title = "third",
                IsComplete = false
            },
            new TodoEntity
            {
                Id = 4,
                PrimaryKey = otherPrimaryKey,
                SortKey = "4",
                Title = "fourth",
                IsComplete = true
            },
        ];
        List<TodoEntity> allEntities = [..expectedEntities, ..otherEntities];
        
        foreach (TodoEntity entity in allEntities)
        {
            await _client.CreateAsync(entity);
        }
        
        // Act
        await _client.DeleteAllAsync(primaryKey);
        
        // Assert
        List<TodoEntity>? removedEntities = await _client.GetAllAsync(primaryKey);
        removedEntities.Should().BeEmpty();

        List<TodoEntity>? remainingEntities = await _client.GetAllAsync(otherPrimaryKey);
        remainingEntities.Should().BeEquivalentTo(otherEntities);
    }
    
    public void Dispose()
    {
        _client.Dispose();
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FacilisDynamodb.Entities;

namespace FacilisDynamoDb.Clients
{
    public interface IFacilisDynamoDbClient<TEntity> : IDisposable where TEntity : class, IIdentity
    {
        Task<TEntity> GetAsync(IIdentity identity, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetAllAsync(string primaryKey, CancellationToken cancellationToken = default);
        Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(IIdentity identity, CancellationToken cancellationToken = default);
        Task DeleteAllAsync(string primaryKey, CancellationToken cancellationToken = default);
    }
}
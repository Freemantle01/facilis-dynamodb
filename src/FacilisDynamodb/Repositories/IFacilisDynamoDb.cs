using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FacilisDynamodb.Entities;

namespace FacilisDynamodb.Repositories
{
    public interface IFacilisDynamoDb<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> GetAsync(IIdentity identity, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(string primaryKey, CancellationToken cancellationToken = default);
        Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(IIdentity identity, CancellationToken cancellationToken = default);
        Task DeleteAllAsync(string primaryKey, CancellationToken cancellationToken = default);
    }
}
using System;
using System.Threading.Tasks;

namespace FacilisDynamoDb.Generators
{
    public interface ITableGenerator : IDisposable
    {
        Task CreateTableAsync();
    }
}
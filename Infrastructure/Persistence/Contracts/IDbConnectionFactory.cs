using System.Data;

namespace Infrastructure.Persistence.Contracts;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
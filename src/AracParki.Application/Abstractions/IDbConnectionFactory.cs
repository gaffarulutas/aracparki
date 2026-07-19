using System.Data;

namespace AracParki.Application.Abstractions;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}

using System.Data.Common;
using BookingDDD.Core.Abstractions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class DapperUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly SqlServerOptions _options;
    private DapperSession? _session;
    private bool _completed;

    public DapperUnitOfWork(SqlServerOptions options)
    {
        _options = options;
    }

    internal async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null)
    {
        var session = await GetSessionAsync();

        return await session.Connection.QuerySingleOrDefaultAsync<T>(
            sql,
            parameters,
            session.Transaction);
    }

    internal async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null)
    {
        var session = await GetSessionAsync();

        var rows = await session.Connection.QueryAsync<T>(
            sql,
            parameters,
            session.Transaction);

        return rows.AsList();
    }

    internal async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null)
    {
        var session = await GetSessionAsync();

        return await session.Connection.ExecuteAsync(
            sql,
            parameters,
            session.Transaction);
    }

    private async Task<DapperSession> GetSessionAsync()
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "This unit of work has already completed.");
        }

        if (_session is null)
        {
            var connection = new SqlConnection(_options.ConnectionString);

            try
            {
                await connection.OpenAsync();
                var transaction =
                    await connection.BeginTransactionAsync();

                _session = new DapperSession(connection, transaction);
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        // A shorter alternative is a named tuple:
        // Task<(SqlConnection Connection, DbTransaction Transaction)>
        // Then the caller can write:
        // var (connection, transaction) = await GetSessionAsync();
        // DapperSession is used here because the two objects share a lifecycle.
        return _session;
    }

    public async Task CommitAsync()
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "This unit of work has already completed.");
        }

        if (_session is not null)
        {
            await _session.Transaction.CommitAsync();
        }

        _completed = true;
        await DisposeSessionAsync();
    }

    public async Task RollbackAsync()
    {
        if (_completed)
        {
            return;
        }

        if (_session is not null)
        {
            await _session.Transaction.RollbackAsync();
        }

        _completed = true;
        await DisposeSessionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            await RollbackAsync();
        }

        await DisposeSessionAsync();
    }

    private async Task DisposeSessionAsync()
    {
        if (_session is null)
        {
            return;
        }

        await _session.DisposeAsync();
        _session = null;
    }

    private sealed class DapperSession(
        SqlConnection connection,
        DbTransaction transaction) : IAsyncDisposable
    {
        public SqlConnection Connection { get; } = connection;
        public DbTransaction Transaction { get; } = transaction;

        public async ValueTask DisposeAsync()
        {
            await Transaction.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}

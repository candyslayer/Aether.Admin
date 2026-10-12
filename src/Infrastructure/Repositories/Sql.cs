using System.Data.Common;
using Npgsql;
using Aether.Application.Interfaces;

namespace Aether.Infrastructure.Repositories;

internal static class Sql
{
    public static NpgsqlCommand Command(NpgsqlConnection c, NpgsqlTransaction? tx, string sql, params (string Name, object? Value)[] args)
    {
        var command = new NpgsqlCommand(sql, c, tx);
        foreach (var (name, value) in args) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }
    public static async Task<List<T>> Query<T>(NpgsqlConnection c, NpgsqlTransaction? tx, string sql, Func<DbDataReader, T> map, CancellationToken ct, params (string Name, object? Value)[] args)
    {
        await using var command = Command(c, tx, sql, args);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var list = new List<T>();
        while (await reader.ReadAsync(ct)) list.Add(map(reader));
        return list;
    }
    public static async Task<int> Execute(NpgsqlConnection c, NpgsqlTransaction? tx, string sql, CancellationToken ct, params (string Name, object? Value)[] args)
    {
        await using var command = Command(c, tx, sql, args); return await command.ExecuteNonQueryAsync(ct);
    }
    public static async Task<T> Scalar<T>(NpgsqlConnection c, NpgsqlTransaction? tx, string sql, CancellationToken ct, params (string Name, object? Value)[] args)
    {
        await using var command = Command(c, tx, sql, args);
        var value = await command.ExecuteScalarAsync(ct);
        return value is T typed ? typed : throw new AppException(404, "Resource not found.");
    }
    public static string? String(DbDataReader r, int n) => r.IsDBNull(n) ? null : r.GetString(n);
    public static Guid? Guid(DbDataReader r, int n) => r.IsDBNull(n) ? null : r.GetGuid(n);
    public static DateTimeOffset Time(DbDataReader r, int n) => new(r.GetDateTime(n), TimeSpan.Zero);
}

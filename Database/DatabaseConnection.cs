using GameCore.Core;
using Npgsql;
using System.Data;
using Dapper;

namespace GameCore.Database;

// database connection managemnet with pooling

public class DatabaseConnection
{
    private readonly string _connectionString;

    public DatabaseConnection(string connectionString)
    {
        _connectionString = connectionString;
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        //test connection
        TestConnection();
    }

    // test the database connection
    private void TestConnection()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT version()";
            var version = cmd.ExecuteScalar()?.ToString();

            Logger.Log(LogLevel.Debug, "DatabaseConn", $"Connected to PostgreSQL: {version?.Split(',')[0]}");
            Logger.Log(LogLevel.Debug, "DatabaseConn", $"Connection pooling enabled");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DatabaseConn", $"Connection failed: {ex.Message}");
            throw;
        }
    }

    // get a database connection (pooled)
    public async Task<IDbConnection> GetConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
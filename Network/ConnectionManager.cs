using System.Collections.Concurrent;
using System.Linq.Expressions;
using GameCore.Core;

namespace GameCore.Network;

//Keeps all the active connections

public class ConnectionManager
{
    // threadsafe dict
    private readonly ConcurrentDictionary<int, Connection> _connections = new();
    private readonly int _maxConnections;
    private readonly object _lock = new object();

    public int ConnectionCount => _connections.Count;
    public bool IsFull => ConnectionCount >= _maxConnections;

    // max connections
    public ConnectionManager(int maxConnections)
    {
        _maxConnections = maxConnections;
    }

    // adds new connection
    public bool Add(Connection connection)
    {
        lock (_lock)
        {
            if (IsFull)
            {
                Logger.Log(LogLevel.Debug,"CONNMGR",$"REJECTED connection #{connection.Id} - Server is full");
                return false;
            }

            if (_connections.TryAdd(connection.Id, connection))
            {
                Logger.Log(LogLevel.Debug,"CONNMGR",$"Added connection #{connection.Id}. Total:{ConnectionCount}");
                return true;
            }
            return false;
        }
    }

    //removes a conncetion
    public void Remove(int connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            connection.Close();
            Logger.Log(LogLevel.Debug,"CONNMGR",$"Removed connection #{connectionId} total:{ConnectionCount}");
        }
    }

    //get specific connection
    public Connection? GetConnection(int connectionId)
    {
        _connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    //broadcast data to everyone
    public async Task BroadcastAsync(byte[] data)
    {
        var tasks = new List<Task>();

        foreach (var connection in _connections.Values)
        {
            tasks.Add(connection.SendAsync(data));
        }

        await Task.WhenAll(tasks);
        Logger.Log(LogLevel.Debug,"CONNMGR",$"Boradcasted {data.Length} bytes to {ConnectionCount} clients");
    }

    // Broadcast till alla utom en specifik connection
    public async Task BroadcastExceptAsync(int exceptConnectionId, byte[] data)
    {
        var connections = GetAll();
        
        foreach (var connection in connections)
        {
            if (connection.Id == exceptConnectionId)
                continue;

            try
            {
                await connection.SendAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONNMGR] Broadcast failed to #{connection.Id}: {ex.Message}");
            }
        }
    }

    //close all connections
    public void DisconnectAll()
    {
        foreach (var conncetion in _connections.Values)
        {
            conncetion.Close();
        }

        _connections.Clear();
        Logger.Log(LogLevel.Debug,"CONNMGR",$"All connections closed");
    }

    //Show status (dubgging)
    public void PrintStatus()
    {
        Console.WriteLine($"\n === CONNECTION STATUS ===");
        Console.WriteLine($"Active connections: {ConnectionCount}/{_maxConnections}");

        foreach (var conn in _connections.Values)
        {
            var uptime = DateTime.UtcNow - conn.ConnectedAt;
            Console.WriteLine($" # {conn.Id} RX={conn.BytesRecived} TX={conn.BytesSent} uptime={uptime.TotalSeconds:F0}s");
            Console.WriteLine($"\n ========================");
        }
    }
}
using System.Net.Sockets;
using GameCore.Core;

namespace GameCore.Network;

// represents a signel client connection
public class Connection
{
    public int Id {get;}
    public TcpClient Client{get;}
    public NetworkStream Stream {get;}
    public DateTime ConnectedAt {get;}
    public DateTime LastAliveUtc { get; private set; }
    public bool IsConnected => Client.Connected;

    // debug stats
    public long BytesRecived {get; private set;}
    public long BytesSent {get; private set;}

    private static int _nextId = 1;

    public Connection(TcpClient client)
    {
        Id = _nextId++;
        Client = client;
        Stream = client.GetStream();
        ConnectedAt = DateTime.UtcNow;
        LastAliveUtc = ConnectedAt;

        Logger.Log(LogLevel.Debug,"CONNECTION",$"#{Id} created from {client.Client.RemoteEndPoint}");
    }

    // lääs data ifrån clienten

    public async Task<byte[]?> ReciveAsync(byte[] buffer)
    {
        try
        {
            int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                return null; // Disconnected

            BytesRecived += bytesRead;

            //copy what we recived
            var data = new byte[bytesRead];
            Array.Copy(buffer, data, bytesRead);
            return data;
        }
        catch
        {
            return null;
        }
    }

    // skicka data till clienten
    public async Task<bool> SendAsync(byte[] data)
    {
        try
        {
            await Stream.WriteAsync(data, 0, data.Length);
            BytesSent += data.Length;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void MarkAlive()
    {
        LastAliveUtc = DateTime.UtcNow;
    }

    //close connection
    public void Close()
    {
        try
        {
            Stream.Close();
            Client.Close();
            Logger.Log(LogLevel.Debug,"CONNECTION",$"#{Id} closed");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Debug,"CONNECTION",$"#{Id} close error: {ex.Message}");
        }
    }
}
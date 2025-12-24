using System.Net;
using System.Net.Sockets;
using System.Text;
using GameCore.Config;
using GameCore.Core;
using GameCore.Network.Packets;
using GameCore.Database.Services;

namespace GameCore.Network;

// Handle TCP connections to the server

public class NetworkManager
{
    private readonly ServerConfig _config;
    private TcpListener? _listener;
    private bool _isRunning;
    private readonly ConnectionManager _connectionManager;

    // database services
    private readonly AccountService _accountService;
    private readonly CharacterService _characterService;

    // track account ID per connection
    private readonly Dictionary<int, int> _connectionAccounts = new();
    //Track character ID per connection
    private readonly Dictionary<int, int> _connectionCharacters = new();

    public NetworkManager(ServerConfig config, AccountService accountService,CharacterService characterService)
    {
        _config = config;
        _accountService = accountService;
        _characterService = characterService;
        _connectionManager = new ConnectionManager(config.MaxConnections);
        Logger.Log(LogLevel.Debug,"NETWORK",$"Manager created");
    }

    //Start TCP-listener
    public void Start()
    {
        if(_isRunning)
        {
            Logger.Log(LogLevel.Debug,"NETWROK",$"Already running...");
            return;
        }

        var IpAddress = IPAddress.Parse(_config.Ip);
        _listener = new TcpListener(IpAddress, _config.Port);
        _listener.Start();
        _isRunning = true;

        Logger.Log(LogLevel.Debug,"NETWORK",$"Listening on {_config.Ip}:{_config.Port}");

        // start acceptloop
        Task.Run(() => AcceptClientsAsync());
        Task.Run(() => MonitorConnectionsAsync());
    }

    private async Task MonitorConnectionsAsync()
    {
        while (_isRunning)
        {
            foreach (var connection in _connectionManager.GetAll())
            {
                if (DateTime.UtcNow - connection.LastAliveUtc > TimeSpan.FromSeconds(15))
                {
                    Logger.Log(
                        LogLevel.Info,
                        "NETWORK",
                        $"Connection #{connection.Id} timed out"
                    );

                    connection.Close();
                    _connectionManager.Remove(connection.Id);
                }
            }

            await Task.Delay(1000); // 1 Hz watchdog
        }
    }

    // Accept new clients (own task)
    private async Task AcceptClientsAsync()
    {
        while(_isRunning)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync();

                // try to creat connection object, if fasle = server full
                var conncetion = new Connection(client);
                if (!_connectionManager.Add(conncetion))
                {
                    await SendServerFullMessage(conncetion);
                    conncetion.Close();
                    continue;
                }

                // Handle client in own task
                _ = Task.Run(() => HandleConnectionAsync(conncetion));
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Logger.Log(LogLevel.Debug,"NETWORK",$"Accept error: {ex.Message}");
                }
            }
        }
    }

    // handle single client
    private async Task HandleConnectionAsync(Connection connection)
    {
        var buffer = new byte[_config.MaxPacketSize];

        try
        {
            while (_isRunning && connection.IsConnected)
            {
                // read data from client
                var data = await connection.ReciveAsync(buffer);

                if (data == null)
                {
                    // disconnect client
                    break;
                }

                Logger.Log(LogLevel.Debug,"NETWORK",$"Connection #{connection.Id} recived {data.Length} bytes");

                //PARSE PACKET 
                try
                {
                    var msg = new NetworkMessage(data);
                    var packetType = PacketHandler.PeekType(msg);

                    await HandlePacketAsync(connection, msg, packetType);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Debug,"NETWORK",$"Connection #{connection.Id} packet error: {ex}");
                    break;
                }

                connection.MarkAlive();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Debug,"NETWORK",$"connection of #{connection.Id} error: {ex.Message}");
        }
        finally
        {
            _connectionAccounts.Remove(connection.Id);
            _connectionCharacters.Remove(connection.Id);
            _connectionManager.Remove(connection.Id);
        }
   }

   //handle specific package based on type
    private async Task HandlePacketAsync(Connection connection, NetworkMessage msg, PacketType type)
    {
        switch(type)
        {
            case PacketType.Ping:
                await HandlePingPacketAsync(connection, msg);
                break;
            case PacketType.Login:
                await HandleLoginPacketAsync(connection, msg);
                break;
            case PacketType.RequestCharList:
                await HandleCharListPacketAsync(connection, msg);
                break;
            case PacketType.SelectCharacter:
                await HandleSelectCharacterAsync(connection, msg);
                break;
            case PacketType.CreateCharacter:
                await HandleCreateCharacterAsync(connection, msg);
                break;
            case PacketType.DeleteCharacter:
                await HandleDeleteCharacterAsync(connection, msg);
                break;
            case PacketType.Chat:
                await HandleChatPacketAsync(connection, msg);
                break;
            case PacketType.Move:
                await HandleMovePacketAsync(connection, msg);
                break;
            default:
                Logger.Log(LogLevel.Debug,"NETWORK",$"Connection #{connection.Id} unknown packet type {type}");
                break;
        }
    }

private async Task HandlePingPacketAsync(Connection connection, NetworkMessage msg)
    {
         long clientTimestamp = PacketHandler.ReadPing(msg);

         var pong = PacketHandler.WritePong(clientTimestamp);
         await connection.SendAsync(pong.GetBytes());

         connection.MarkAlive();
    }

private async Task HandleLoginPacketAsync(Connection connection, NetworkMessage msg)
{
    var packet = PacketHandler.ReadLogin(msg);

    Logger.Log(LogLevel.Info, "NETWORK", $"Connection #{connection.Id} login attempt: {packet.Username}");

    // Version check
    if (packet.ClientVersion != Consts.CLIENT_VERSION)
    {
        var response = PacketHandler.WriteLoginFailed(
            $"Version mismatch. Server requires v{Consts.CLIENT_VERSION}, you have v{packet.ClientVersion}"
        );
        await connection.SendAsync(response.GetBytes());
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} rejected: wrong client version v{packet.ClientVersion}");
        return;
    }

    // ===== Validate via database (flyttat validering till AccountService) =====
    var account = await _accountService.ValidateLoginAsync(packet.Username, packet.Password);

    if (account == null)
    {
        var response = PacketHandler.WriteLoginFailed("Invalid username or password");
        await connection.SendAsync(response.GetBytes());
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} login failed: invalid credentials");
        return;
    }

    // save accountid
    _connectionAccounts[connection.Id] = account.Id;

    var motd = PacketHandler.WriteLoginResponse(_config.Motd);
    await connection.SendAsync(motd.GetBytes());

    Logger.Log(LogLevel.Info, "NETWORK", $"Connection #{connection.Id} login success: {packet.Username}");
    // fetchcharlist  own function
}

private async Task HandleCharListPacketAsync(Connection connection, NetworkMessage msg)
{
    PacketHandler.ReadRequestCharList(msg);

    // Hämta account ID från connection mapping
    if (!_connectionAccounts.TryGetValue(connection.Id, out var accountId))
    {
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} requested char list without login");
        return;
    }
    
    Logger.Log(LogLevel.Debug, "NETWORK", $"Connection #{connection.Id} requesting character list");
    

    // Fetch characters from database
    var dbCharacters = await _characterService.GetCharactersByAccountAsync(accountId);

    // Convert DB models → packet format
    var characters = dbCharacters.Select(c => new CharacterData
    {
        Id = c.Id,
        Name = c.Name,
        Level = c.Level,
        Class = c.Class
    }).ToList();

    // Send character list
    var charListResponse = PacketHandler.WriteCharacterList(characters);
    await connection.SendAsync(charListResponse.GetBytes());

    Logger.Log(LogLevel.Info, "NETWORK", $"Connection #{connection.Id} Character list sent: ({characters.Count} characters)");
}

/// Hantera character selection
private async Task HandleSelectCharacterAsync(Connection connection, NetworkMessage msg)
{
    var packet = PacketHandler.ReadSelectCharacter(msg);

    if (!_connectionAccounts.TryGetValue(connection.Id, out var accountId))
    {
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} tried to select character without login");
        return;
    }

    var character = await _characterService.GetCharacterAsync(packet.CharacterId, accountId);

    if (character == null)
    {
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} tried to select invalid character {packet.CharacterId}");
        return;
    }

    Logger.Log(LogLevel.Info, "NETWORK", $"Connection #{connection.Id} selected character: {character.Name}");

    // Spara character ID
    _connectionCharacters[connection.Id] = character.Id;

    // Update last login
    await _characterService.UpdateLastLoginAsync(character.Id);

    // Convert DB model → packet format
    var characterData = new CharacterData
    {
        Id = character.Id,
        Name = character.Name,
        Level = character.Level,
        Class = character.Class
    };

    // Send confirmation
    var response = PacketHandler.WriteCharacterSelected(characterData);
    await connection.SendAsync(response.GetBytes());

    // ===== NY: Skicka player spawn (sin egen position) =====
    var spawnPacket = PacketHandler.WritePlayerSpawn(
        connection.Id,
        character.Name,
        character.PositionX,
        character.PositionY
    );
    await connection.SendAsync(spawnPacket.GetBytes());

    // ===== NY: Skicka alla andra spelare som redan är inne =====
    await SendExistingPlayersAsync(connection);

    // ===== NY: Broadcasta till alla ANDRA att denna player spawnade =====
    await _connectionManager.BroadcastExceptAsync(
        connection.Id,
        spawnPacket.GetBytes()
    );

    Logger.Log(LogLevel.Info, "NETWORK", $"Player {character.Name} spawned at ({character.PositionX}, {character.PositionY})");
}

/// Hantera create character
private async Task HandleCreateCharacterAsync(Connection connection, NetworkMessage msg)
{
    var packet = PacketHandler.ReadCreateCharacter(msg);

    if (!_connectionAccounts.TryGetValue(connection.Id, out var accountId))
    {
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} tried to create character without login");
        return;
    }

    // Create character in database (validation happens in CharacterService)
    var character = await _characterService.CreateCharacterAsync(accountId, packet.Name, packet.Class);

    if (character == null)
    {
        // Send error
        var errorMsg = PacketHandler.WriteSystemMessage("Failed to create character. Name may be taken or invalid.");
        await connection.SendAsync(errorMsg.GetBytes());
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} failed to create character: {packet.Name}");
        return;
    }

    // Convert DB model → packet format
    var characterData = new CharacterData
    {
        Id = character.Id,
        Name = character.Name,
        Level = character.Level,
        Class = character.Class
    };

    // Send confirmation
    var response = PacketHandler.WriteCharacterCreated(characterData);
    await connection.SendAsync(response.GetBytes());

    Logger.Log(LogLevel.Info, "NETWORK", $"Connection #{connection.Id} created character: {character.Name}");
}

/// Hantera delete character
private async Task HandleDeleteCharacterAsync(Connection connection, NetworkMessage msg)
{
    var packet = PacketHandler.ReadDeleteCharacter(msg);

    if (!_connectionAccounts.TryGetValue(connection.Id, out var accountId))
    {
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} tried to delete character without login");
        return;
    }

    // ===== TODO: Verify password här (lägg till metod i AccountService) =====
    // För nu: Skippa password check
    
    // Soft delete in database
    bool success = await _characterService.DeleteCharacterAsync(packet.CharacterId, accountId);

    string message = success 
        ? "Character deleted (will be permanently removed after 30 days)" 
        : "Character not found or already deleted";

    var response = PacketHandler.WriteCharacterDeleted(packet.CharacterId, success, message);
    await connection.SendAsync(response.GetBytes());

    Logger.Log(LogLevel.Info, "NETWORK", $"Connection #{connection.Id} delete character {packet.CharacterId}: {(success ? "SUCCESS" : "FAILED")}");
}

// get character name for a connection
private async Task<string?> GetCharacterNameAsync(int connectionId)
{
    if (!_connectionCharacters.TryGetValue(connectionId, out var characterId))
        return null;

    if (!_connectionAccounts.TryGetValue(connectionId, out var accountId))
        return null;

    var character = await _characterService.GetCharacterAsync(characterId, accountId);
    return character?.Name;
}

    //handle chat packeges
private async Task HandleChatPacketAsync(Connection connection, NetworkMessage msg)
{
    var packet = PacketHandler.ReadChat(msg);

    // ===== ÄNDRAT: Hämta character name från database =====
    string? playerName = await GetCharacterNameAsync(connection.Id);

    if (playerName == null)
    {
        Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} tried to chat without character selected");
        return;
    }

    Logger.Log(LogLevel.Debug, "NETWORK", $"[{playerName}] {packet.Message}");

    // Broadcast to everyone
    var broadcast = PacketHandler.WriteChatBroadcast(playerName, packet.Message);
    await _connectionManager.BroadcastAsync(broadcast.GetBytes());
}

    //handle move package
    private async Task HandleMovePacketAsync(Connection connection, NetworkMessage msg)
    {
        var packet = PacketHandler.ReadMove(msg);

        // Validera att character är selected
        if (!_connectionCharacters.TryGetValue(connection.Id, out var characterId))
        {
            Logger.Log(LogLevel.Error, "NETWORK", $"Connection #{connection.Id} tried to move without character selected");
            return;
        }

        Logger.Log(LogLevel.Debug, "NETWORK", $"Connection #{connection.Id} move to: {packet.TargetX}, {packet.TargetY}");

        // TODO: Validera movement (är positionen giltig? har spelaren stamina? etc)

        // Spara position i database
        await _characterService.UpdatePositionAsync(characterId, packet.TargetX, packet.TargetY);

        // Broadcast till ALLA (inklusive avsändaren för bekräftelse)
        var broadcast = PacketHandler.WritePlayerMove(connection.Id, packet.TargetX, packet.TargetY);
        await _connectionManager.BroadcastAsync(broadcast.GetBytes());

        connection.MarkAlive();
    }

    private async Task SendExistingPlayersAsync(Connection newConnection)
    {
        foreach (var kvp in _connectionCharacters)
        {
            int existingConnectionId = kvp.Key;
            int existingCharacterId = kvp.Value;

            // Skippa den nya spelaren själv
            if (existingConnectionId == newConnection.Id)
                continue;

            // Hämta character data
            if (!_connectionAccounts.TryGetValue(existingConnectionId, out var accountId))
                continue;

            var character = await _characterService.GetCharacterAsync(existingCharacterId, accountId);
            if (character == null)
                continue;

            // Skicka PlayerSpawn för denna existerande spelare
            var spawnPacket = PacketHandler.WritePlayerSpawn(
                existingConnectionId,
                character.Name,
                character.PositionX,
                character.PositionY
            );

            await newConnection.SendAsync(spawnPacket.GetBytes());
        }
    }

    //send "server full" message to clinet before close connection
    private async Task SendServerFullMessage(Connection connection)
    {
        try
        {
            var message = Encoding.UTF8.GetBytes("Server is full, try again later.\n");
            await connection.SendAsync(message);
        }
        catch
        {
            // ignore
        }
    }

   // stop server
   public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _connectionManager.DisconnectAll();
        _listener?.Stop();
        Logger.Log(LogLevel.Debug,"NETWORK",$"stopped");
    }

    public void ShowStatus()
    {
        _connectionManager.PrintStatus();
    }
}
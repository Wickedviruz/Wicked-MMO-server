using GameCore.Core;

namespace GameCore.Network.Packets;

/// <summary>
/// Centraliserad packet läsning/skrivning
/// TFS-inspirerad: Alla read/write metoder på ett ställe
/// </summary>
public static class PacketHandler
{
    // ============================================
    // WRITE METHODS (Server → Client)
    // ============================================
    
    public static NetworkMessage WriteLoginResponse(string welcomeMessage)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.LoginResponse);
        msg.AddString(welcomeMessage);
        return msg;
    }
    
    public static NetworkMessage WriteLoginFailed(string reason)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.LoginFailed);
        msg.AddString(reason);
        return msg;
    }
    
    public static NetworkMessage WriteChatBroadcast(string fromPlayer, string message)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.ChatBroadcast);
        msg.AddString(fromPlayer);
        msg.AddString(message);
        return msg;
    }
    
    public static NetworkMessage WriteSystemMessage(string message)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.SystemMessage);
        msg.AddString(message);
        return msg;
    }
    
    public static NetworkMessage WritePlayerMove(int playerId, int x, int y)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.PlayerMove);
        msg.AddInt32(playerId);
        msg.AddInt32(x);
        msg.AddInt32(y);
        return msg;
    }
    
    public static NetworkMessage WritePlayerSpawn(int playerId, string name, int x, int y)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.PlayerSpawn);
        msg.AddInt32(playerId);
        msg.AddString(name);
        msg.AddInt32(x);
        msg.AddInt32(y);
        return msg;
    }
    
    public static NetworkMessage WritePlayerDespawn(int playerId)
    {
        var msg = new NetworkMessage();
        msg.AddByte((byte)PacketType.PlayerDespawn);
        msg.AddInt32(playerId);
        return msg;
    }
    
    // ============================================
    // READ METHODS (Client -> Server)
    // ============================================
    
    public static LoginPacket ReadLogin(NetworkMessage msg)
    {
        msg.GetByte(); // Skip packet type

        return new LoginPacket
        {
            Username = msg.GetString(),
            Password = msg.GetString(),
            ClientVersion = msg.GetString()
        };
    }
    
    public static ChatPacket ReadChat(NetworkMessage msg)
    {
        msg.GetByte(); // Skip packet type
        
        var message = msg.GetString();
        
        if (message.Length > Consts.MAX_CHAT_MESSAGE_LENGTH)
        {
            throw new Exception($"Chat message too long: {message.Length}");
        }
        
        return new ChatPacket
        {
            Message = message
        };
    }
    
    public static MovePacket ReadMove(NetworkMessage msg)
    {
        msg.GetByte(); // Skip packet type
        
        var x = msg.GetInt32();
        var y = msg.GetInt32();
        
        return new MovePacket
        {
            TargetX = x,
            TargetY = y
        };
    }

    public static void ReadRequestCharList(NetworkMessage msg)
    {
        msg.GetByte();
    }

    // ============================================
// CHARACTER SELECTION - WRITE (Server → Client)
// ============================================

public static NetworkMessage WriteCharacterList(List<CharacterData> characters)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.CharacterList);
    msg.AddByte((byte)characters.Count);
    
    foreach (var character in characters)
    {
        msg.AddInt32(character.Id);
        msg.AddString(character.Name);
        msg.AddInt32(character.Level);
        msg.AddString(character.Class);
    }
    
    return msg;
}

public static NetworkMessage WriteCharacterSelected(CharacterData character)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.CharacterSelected);
    msg.AddInt32(character.Id);
    msg.AddString(character.Name);
    msg.AddInt32(character.Level);
    msg.AddString(character.Class);
    return msg;
}

public static NetworkMessage WriteCharacterCreated(CharacterData character)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.CharacterCreated);
    msg.AddInt32(character.Id);
    msg.AddString(character.Name);
    msg.AddInt32(character.Level);
    msg.AddString(character.Class);
    return msg;
}

public static NetworkMessage WriteCharacterDeleted(int characterId, bool success, string message)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.CharacterDeleted);
    msg.AddInt32(characterId);
    msg.AddBool(success);
    msg.AddString(message);
    return msg;
}

// ============================================
// CHARACTER SELECTION - WRITE (Client → Server)
// ============================================

public static NetworkMessage WriteSelectCharacter(int characterId)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.SelectCharacter);
    msg.AddInt32(characterId);
    return msg;
}

public static NetworkMessage WriteCreateCharacter(string name, string characterClass)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.CreateCharacter);
    msg.AddString(name);
    msg.AddString(characterClass);
    return msg;
}

public static NetworkMessage WriteDeleteCharacter(int characterId, string password)
{
    var msg = new NetworkMessage();
    msg.AddByte((byte)PacketType.DeleteCharacter);
    msg.AddInt32(characterId);
    msg.AddString(password);
    return msg;
}

// ============================================
// CHARACTER SELECTION - READ (Server → Client)
// ============================================

public static CharacterListPacket ReadCharacterList(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    
    var packet = new CharacterListPacket();
    int count = msg.GetByte();
    
    for (int i = 0; i < count; i++)
    {
        var character = new CharacterData
        {
            Id = msg.GetInt32(),
            Name = msg.GetString(),
            Level = msg.GetInt32(),
            Class = msg.GetString()
        };
        
        packet.Characters.Add(character);
    }
    
    return packet;
}

public static CharacterSelectedPacket ReadCharacterSelected(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    
    return new CharacterSelectedPacket
    {
        Character = new CharacterData
        {
            Id = msg.GetInt32(),
            Name = msg.GetString(),
            Level = msg.GetInt32(),
            Class = msg.GetString()
        }
    };
}

public static CharacterCreatedPacket ReadCharacterCreated(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    
    return new CharacterCreatedPacket
    {
        Character = new CharacterData
        {
            Id = msg.GetInt32(),
            Name = msg.GetString(),
            Level = msg.GetInt32(),
            Class = msg.GetString()
        }
    };
}

public static CharacterDeletedPacket ReadCharacterDeleted(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    
    return new CharacterDeletedPacket
    {
        CharacterId = msg.GetInt32(),
        Success = msg.GetBool(),
        Message = msg.GetString()
    };
}

// ============================================
// CHARACTER SELECTION - READ (Client → Server)
// ============================================

public static SelectCharacterPacket ReadSelectCharacter(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    return new SelectCharacterPacket
    {
        CharacterId = msg.GetInt32()
    };
}

public static CreateCharacterPacket ReadCreateCharacter(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    return new CreateCharacterPacket
    {
        Name = msg.GetString(),
        Class = msg.GetString()
    };
}

public static DeleteCharacterPacket ReadDeleteCharacter(NetworkMessage msg)
{
    msg.GetByte(); // Skip type
    return new DeleteCharacterPacket
    {
        CharacterId = msg.GetInt32(),
        Password = msg.GetString()
    };
}
    
    // ============================================
    // UTILITY
    // ============================================
    
    /// Kolla packet type utan att konsumera byten
    public static PacketType PeekType(NetworkMessage msg)
    {
        var currentPos = msg.Position;
        var type = (PacketType)msg.GetByte();
        msg.Reset();
        return type;
    }
    
    /// Kolla packet type och konsumera byten
    public static PacketType ReadType(NetworkMessage msg)
    {
        return (PacketType)msg.GetByte();
    }
}
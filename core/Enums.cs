/// Core game enums (TFS-style: enums.h)
namespace GameCore.Core;

// =================================
// Game state
// =================================

public enum GameState
{
    Stopped,
    Initializing,
    Ready,
    Running,
    Shutdown
}

// =================================
// DIRECTION
// =================================
public enum Direction
{
    North,
    East,
    South,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest,
}

// =================================
// DAMAGE TYPES
// =================================
public enum DamageType
{
    Physical,
    Energy,
    Fire,
    Ice,
    Poison,
    Holy,
    Death,
}

// =================================
// RACE TYPES
// =================================
public enum RaceTypes
{
    None,
    Blood,
    Venom,
    Undead,
    Fire,
    Energy,
}

// =================================
// TILE TYPES
// =================================
public enum TileType
{
    None,
    Grass,
    Stone,
    Water,
    Wall,
    Sand,
    Lava
}


// =================================
// CONNECTION STATE
// =================================

public enum ConnectionState
{
    CONNECTION_STATE_DISCONNECTED,
    CONNECTION_STATE_REQUEST_CHARLIST,
    CONNECTION_STATE_GAMEWORLD_AUTH,
    CONNECTION_STATE_GAME,
    CONNECTION_STATE_PENDING
}

// =================================
// NET PACKETS TYPES
// =================================

public enum PacketType : byte
{
    //Client -> Server
    Login = 0x01,
    Logout = 0x02,
    Chat = 0x03,
    Move = 0x04,
    CreateCharacter = 0x05,
	SelectCharacter = 0x06,
	DeleteCharacter = 0x07,
    RequestCharList = 0x08,
    
    
    //Server -> Client
    LoginResponse = 0x10,
    LoginFailed = 0x11,
    ChatBroadcast = 0x12,
    SystemMessage = 0x13,
    PlayerMove = 0x14,
    PlayerSpawn = 0x15,
    PlayerDespawn = 0x16,
    CharacterList = 0x17,
	CharacterCreated = 0x18, 
	CharacterSelected = 0x19,
    CharacterDeleted = 0x20,  
    
    //Utility
    Ping = 0xFE,
    Pong = 0xFF
}
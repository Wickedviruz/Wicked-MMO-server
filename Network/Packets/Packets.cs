namespace GameCore.Network.Packets;

// ============================================
// CLIENT → SERVER PACKETS
// ============================================

/// Client försöker logga in
public class LoginPacket
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ClientVersion { get; set; } = "";
}

// character data
public class CharacterData
{
    public int Id {get;set;}
    public string Name {get;set;} ="";
    public int Level {get;set;} =1;
    public string Class {get;set;}="Warrior";
}

// choose character
public class SelectCharacterPacket
{
    public int CharacterId { get; set; }
}

// create character
public class CreateCharacterPacket
{
    public string Name { get; set; } = "";
    public string Class { get; set; } = "Warrior";
}

// delete character
public class DeleteCharacterPacket
{
    public int CharacterId { get; set; }
    public string Password { get; set; } = ""; // Bekräftelse
}


/// Client skickar chat-meddelande
public class ChatPacket
{
    public string Message { get; set; } = "";
}

/// Client vill flytta sin karaktär
public class MovePacket
{
    public int TargetX { get; set; }
    public int TargetY { get; set; }
}

// ============================================
// SERVER → CLIENT PACKETS
// ============================================

/// Resultat av login-försök (success)
public class LoginResponsePacket
{
    public string WelcomeMessage { get; set; } = "";
}

/// Login misslyckades
public class LoginFailedPacket
{
    public string Reason { get; set; } = "";
}

// List of characters after login
public class CharacterListPacket
{
    public List<CharacterData> Characters {get; set;} = new();
}

// character seleceted
public class CharacterSelectedPacket
{
    public CharacterData Character { get; set; } = new();
}

//character created
public class CharacterCreatedPacket
{
    public CharacterData Character { get; set; } = new();
}

// character  deleted
public class CharacterDeletedPacket
{
    public int CharacterId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}

/// Chat från en spelare (broadcast till alla)
public class ChatBroadcastPacket
{
    public string FromPlayer { get; set; } = "";
    public string Message { get; set; } = "";
}

/// System-meddelande från servern
public class SystemMessagePacket
{
    public string Message { get; set; } = "";
}

/// En spelare har flyttat
public class PlayerMovePacket
{
    public int PlayerId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

/// En spelare har spawnat (kom in i världen)
public class PlayerSpawnPacket
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
}

/// En spelare har despawnat (loggat ut)
public class PlayerDespawnPacket
{
    public int PlayerId { get; set; }
}
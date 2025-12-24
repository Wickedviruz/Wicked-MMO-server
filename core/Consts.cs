/// Core game consts
namespace GameCore.Core;

// server wide constants

public static class Consts
{
    public const int NETWORKMESSAGE_MAXSIZE = 24590;
    public const ushort PROTOCOL_VERSION = 1;
    public const string SERVER_NAME = "Wicked Emulator";
    public const string SERVER_DEVELOPERS = "Wickedviruz";
    public const string SERVER_VERSION = "0.1.0";
    public const string CLIENT_VERSION = "0.1.0";
    public const int MAX_USERNAME_LENGTH = 255;
    public const int MAX_PASSWORD_LENGTH = 255;
    public const int MAX_CHAT_MESSAGE_LENGTH = 255;
}
using GameCore.Config;
using GameCore.Database;
using GameCore.Database.Services;
using GameCore.Network;

namespace GameCore.Core;

/// <summary>
/// Main game singleton. Manages entire server state.
/// Inspired by TFS Game class, but with idiomatic C#.
/// </summary>

public sealed class Game
{
    // singelton instance
    private static readonly Lazy<Game> _instance = new(() => new Game());
    public static Game Instance => _instance.Value;

    // state
    public GameState State {get; private set;} = GameState.Stopped;

    //managers
    private ServerConfig? _config;
    private NetworkManager? _networkManager;

    // database
    private DatabaseConnection? _dbConnection;
    private AccountService? _accountService;
    private CharacterService? _characterService;

    private Game()
    {
        Logger.Log(LogLevel.Debug,"GAME",$"Instance created");
    }

    public bool Initialize()
    {
        Logger.Log(LogLevel.Debug,"GAME",$"Initializing...");
        State = GameState.Initializing;
        
        try
        {
            // load config file
            Logger.Log(LogLevel.Info, "GAME", "Loading configuration...");
            _config = ServerConfig.Load();

            // database connection
            Logger.Log(LogLevel.Info, "GAME", "Connecting to database...");
            try
            {
                _dbConnection = new DatabaseConnection(_config.GetConnectionString());
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "GAME", "FATAL: Database connection failed!");
                Logger.Log(LogLevel.Error, "GAME", $"Error: {ex.Message}");
                Logger.Log(LogLevel.Error, "GAME", "Server cannot start without database.");
                return false;  // ← EXIT
            }

            // Initialize services
            Logger.Log(LogLevel.Info, "GAME", "Initializing database services...");
            _accountService = new AccountService(_dbConnection);
            _characterService = new CharacterService(_dbConnection);

            // Initialize network
            Logger.Log(LogLevel.Info, "GAME", "Initializing network...");
            _networkManager = new NetworkManager(_config, _accountService, _characterService);

            // TODO: Create map
            // TODO: Initialize systems

            State = GameState.Ready;
            Logger.Log(LogLevel.Info, "GAME", "Initialization complete!");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "GAME", "FATAL: Initialization failed!");
            Logger.Log(LogLevel.Error, "GAME", $"Error: {ex.Message}");
            Logger.Log(LogLevel.Error, "GAME", $"Stack trace: {ex.StackTrace}");
            return false;  // ← EXIT
        }
    }

    /// start game loop
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (State != GameState.Ready)
        {
            Logger.Log(LogLevel.Debug,"GAME",$"Cannot start - not ready!");
            return;
        }

        State = GameState.Running;
        Logger.Log(LogLevel.Debug,"GAME",$"Running...");

        // TODO: Start game loop
        _networkManager!.Start();

        try
        {
            // vänta tills cancelled
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Logger.Log(LogLevel.Debug,"GAME",$"Shutdown requested");
        }
    }

    public void Stop()
    {
        _networkManager?.Stop();
        State = GameState.Stopped;
        Logger.Log(LogLevel.Debug,"GAME",$"Stopped...");
    }
}
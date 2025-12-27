using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MoonSharp.Interpreter;
using GameCore.Config;
using GameCore.Database;
using GameCore.Database.Services;
using GameCore.Network;
using GameCore.Loaders;

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
        printServerVersion();
    }

/// <summary>
/// Starts the game server and initializes all subsystems.
/// </summary>
    public bool Initialize()
    {
        State = GameState.Initializing;
        //Load config,
        //connect to DB,
        //Load 
        try
        {
            // load config file
            Logger.Log(LogLevel.Info, "CORE", "Loading configuration...");
            _config = ServerConfig.Load();
            Logger.Log(LogLevel.Info, "CORE", "configuration loaded");

            // database connection
            Logger.Log(LogLevel.Info, "CORE", "Connecting to database...");
            try
            {
                _dbConnection = new DatabaseConnection(_config.GetConnectionString());
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CORE", "FATAL: Database connection failed!");
                Logger.Log(LogLevel.Error, "CORE", $"Error: {ex.Message}");
                Logger.Log(LogLevel.Error, "CORE", "Server cannot start without database.");
                return false;  // ← EXIT
            }

            // Initialize services
            Logger.Log(LogLevel.Info, "CORE", "Initializing database services...");
            _accountService = new AccountService(_dbConnection);
            _characterService = new CharacterService(_dbConnection);

            // Initialize network
            Logger.Log(LogLevel.Info, "CORE", "Initializing network...");
            _networkManager = new NetworkManager(_config, _accountService, _characterService);

            // TODO: Initialize systems
            TryLoad("monsters", () => Loaders.Monsters.MonsterLoader.Load("data/monster"));
            // TODO: Create map
            

            State = GameState.Ready;
            Logger.Log(LogLevel.Info, "CORE", "Initialization complete!");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "CORE", "FATAL: Initialization failed!");
            Logger.Log(LogLevel.Error, "CORE", $"Error: {ex.Message}");
            Logger.Log(LogLevel.Error, "CORE", $"Stack trace: {ex.StackTrace}");
            return false;  // EXIT
        }
    }

    private void TryLoad(string name, Action loader)
    {
        try
        {
            Logger.Log(LogLevel.Info, "CORE", $"Loading {name}...");
            loader();
            Logger.Log(LogLevel.Info, "CORE", $"{name} loaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "CORE", $"Failed to load {name}");
            Logger.Log(LogLevel.Error, "CORE", ex.Message);
        }
    }

    /// start game loop
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (State != GameState.Ready)
        {
            Logger.Log(LogLevel.Debug,"CORE",$"Cannot start - not ready!");
            return;
        }

        _networkManager!.Start();

        State = GameState.Running;
        Logger.Log(LogLevel.Info,"CORE",$"Running...");

        try
        {
            // vänta tills cancelled
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Logger.Log(LogLevel.Debug,"CORE",$"Shutdown requested");
        }
    }
/// <summary>
/// Prints server version, compiled version, and runtime info
/// And then the lua version used
/// and last the developers team
/// </summary>
    public void printServerVersion()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var version = assembly.GetName().Version?.ToString() ?? "unknown";

        Logger.Log(LogLevel.Info, "CORE",
            $"Wicked Server - Version {Consts.SERVER_VERSION}");

        Logger.Log(LogLevel.Info, "CORE",
            $"Compiled on {GetBuildDate()} for platform {RuntimeInformation.ProcessArchitecture}");

        Logger.Log(LogLevel.Info, "CORE",
            $"Runtime: {RuntimeInformation.FrameworkDescription}");

        Logger.Log(LogLevel.Info, "CORE",
            $"Lua: Lua 5.2 (emulated) via MoonSharp {typeof(Script).Assembly.GetName().Version}");

        Logger.Log(LogLevel.Info, "CORE",$"A server developed by {Consts.SERVER_DEVELOPERS}\n");

    }

    public void Stop()
    {
        _networkManager?.Stop();
        State = GameState.Stopped;
        Logger.Log(LogLevel.Debug,"CORE",$"Stopped...");
    }

    private static string GetBuildDate()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            return "unknown";

        foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (attr.Key == "BuildDate")
                return attr.Value ?? "unknown";
        }

        return "unknown";
    }
}
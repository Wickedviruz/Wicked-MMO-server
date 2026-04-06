using System.ComponentModel;
using GameCore.Core;
using GameCore.Database;
using GameCore.Database.Services;

// ===== NY: Check for admin commands =====
if (args.Length > 0)
{
    switch (args[0].ToLower())
    {
        case "createaccount":
            await CreateAccountCommandAsync();
            return 0;
        
        case "help":
            ShowHelp();
            return 0;
        
        default:
            Console.WriteLine($"Unknown command: {args[0]}");
            Console.WriteLine("Use 'help' for available commands");
            return 1;
    }
}

// get game instance
var game = Game.Instance;

// Initialize (CRITICAL - exit if fails)
if (!game.Initialize())
{
    Console.WriteLine("\n=== SERVER INITIALIZATION FAILED ===");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return 1;  // Exit code 1 = error
}


// Start (wait forever)
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    game.Stop();
};

await game.StartAsync(cts.Token);
return 0;


// ===== Admin Commands =====

async Task CreateAccountCommandAsync()
{
    Console.WriteLine("=== Create Account ===\n");

    Console.Write("Database password: ");
    var dbPassword = ReadPassword();
    
    var connectionString = $"Host=localhost;Port=5432;Database=wicked_mmorpg;Username=postgres;Password={dbPassword}";

    try
    {
        var db = new DatabaseConnection(connectionString);
        var accountService = new AccountService(db);

        Console.Write("Username: ");
        var username = Console.ReadLine() ?? "testuser";

        Console.Write("Password: ");
        var password = ReadPassword(); 

        Console.Write("Email (optional): ");
        var email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email)) email = null;

        Console.WriteLine("\nCreating account...");
        var account = await accountService.CreateAccountAsync(username, password, email);

        if (account != null)
        {
            Console.WriteLine($"\n Account created!");
            Console.WriteLine($"  Username: {account.Username}");
            Console.WriteLine($"  ID: {account.Id}");
        }
        else
        {
            Console.WriteLine("\n✗ Failed (username may exist)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n Error: {ex.Message}");
    }
}

static string ReadPassword()
{
    var password = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }
        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Remove(password.Length - 1, 1);
            Console.WriteLine("\b \b");
        }
        else if (key.Key != ConsoleKey.Backspace)
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    }
    return password.ToString();
}

void ShowHelp()
{
    Console.WriteLine("Available commands:");
    Console.WriteLine("  dotnet run                    - Start server");
    Console.WriteLine("  dotnet run createaccount      - Create test account");
    Console.WriteLine("  dotnet run help               - Show this help");
}
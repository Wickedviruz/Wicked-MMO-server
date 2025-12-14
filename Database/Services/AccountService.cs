using Dapper;
using GameCore.Core;
using GameCore.Database.Models;

namespace GameCore.Database.Services;

// account services - handles account operations ( login, register ect)

public class AccountService
{
    private readonly DatabaseConnection _db;

    public AccountService(DatabaseConnection db)
    {
        _db = db;
    }

    // account validation on login credentials
    public async Task<Account?> ValidateLoginAsync(string username, string password)
    {
        // ===== Validering av input =====
        if (string.IsNullOrWhiteSpace(username) || username.Length > 32)
        {
            Console.WriteLine($"[ACCOUNT] Login failed: Invalid username format");
            return null;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length > 255)
        {
            Console.WriteLine($"[ACCOUNT] Login failed: Invalid password format");
            return null;
        }
        

        var sql = "SELECT * FROM accounts WHERE LOWER(username) = LOWER(@Username)";
        
        using var connection = await _db.GetConnectionAsync();
        
        var account = await connection.QuerySingleOrDefaultAsync<Account>(sql, new { Username = username });
        
        if (account == null)
        {
            Console.WriteLine($"[ACCOUNT] Login failed: Account '{username}' not found");
            return null;
        }
        
        if (string.IsNullOrEmpty(account.PasswordHash))
        {
            Console.WriteLine($"[ACCOUNT] Login failed: Password hash is null or empty!");
            return null;
        }

        // Verify password
        try
        {
            bool verified = BCrypt.Net.BCrypt.Verify(password, account.PasswordHash);
            Console.WriteLine($"[ACCOUNT] DEBUG: Password verify result: {verified}");
            
            if (!verified)
            {
                Console.WriteLine($"[ACCOUNT] Login failed: Invalid password for '{username}'");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ACCOUNT] Login failed: BCrypt error: {ex.Message}");
            return null;
        }
        
        // Update last login
        await UpdateLastLoginAsync(account.Id);
        
        Console.WriteLine($"[ACCOUNT] Login success: {username} (ID: {account.Id})");
        return account;
    }

    // Create new account
    public async Task<Account?> CreateAccountAsync(string username, string password, string? email = null)
    {
        // Hash password
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        
        var sql = @"
            INSERT INTO accounts (username, password_hash, email, created_at)
            VALUES (@Username, @PasswordHash, @Email, @CreatedAt)
            RETURNING id, username, password_hash, email, created_at, last_login";
        
        using var connection = await _db.GetConnectionAsync();
        
        try
        {
            var account = await connection.QuerySingleAsync<Account>(sql, new
            {
                Username = username,
                PasswordHash = passwordHash,
                Email = email,
                CreatedAt = DateTime.UtcNow
            });
            
            Logger.Log(LogLevel.Debug, "ACCOUNT",$"Created: {username} (ID: {account.Id})");
            return account;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") // Unique violation
        {
            Logger.Log(LogLevel.Debug, "ACCOUNT",$"Create failed: Username '{username}' already exists");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ACCOUNT",$"Create failed: {ex.Message}");
            return null;
        }
    }
    
    // Check if account exists

    public async Task<bool> AccountExistsAsync(string username)
    {
        var sql = "SELECT COUNT(*) FROM accounts WHERE LOWER(username) = LOWER(@Username)";
        
        using var connection = await _db.GetConnectionAsync();
        
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
        return count > 0;
    }


    /// Update last login timestamp
    private async Task UpdateLastLoginAsync(int accountId)
    {
        var sql = "UPDATE accounts SET last_login = @Now WHERE id = @Id";
        
        using var connection = await _db.GetConnectionAsync();
        await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow, Id = accountId });
    }
}
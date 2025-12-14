using Dapper;
using GameCore.Database.Models;
using GameCore.Core;

namespace GameCore.Database.Services;

/// Character service - handles character operations (create, load, save, delete)
public class CharacterService
{
    private readonly DatabaseConnection _db;
    
    public CharacterService(DatabaseConnection db)
    {
        _db = db;
    }
    
    /// Get all active characters for an account
    public async Task<List<Character>> GetCharactersByAccountAsync(int accountId)
    {
        var sql = @"
            SELECT * FROM characters 
            WHERE account_id = @AccountId 
              AND deleted_at IS NULL
            ORDER BY created_at ASC";
        
        using var connection = await _db.GetConnectionAsync();
        
        var characters = await connection.QueryAsync<Character>(sql, new { AccountId = accountId });
        Logger.Log(LogLevel.Debug, "CHARACTER",$"Loaded {characters.Count()} characters for account {accountId}");
        
        return characters.ToList();
    }
    
    /// Get a specific character by ID (if owned by account)
    public async Task<Character?> GetCharacterAsync(int characterId, int accountId)
    {
        var sql = @"
            SELECT * FROM characters 
            WHERE id = @CharacterId 
              AND account_id = @AccountId 
              AND deleted_at IS NULL";
        
        using var connection = await _db.GetConnectionAsync();
        
        return await connection.QuerySingleOrDefaultAsync<Character>(sql, new 
        { 
            CharacterId = characterId, 
            AccountId = accountId 
        });
    }
    
    /// Create new character
    public async Task<Character?> CreateCharacterAsync(int accountId, string name, string characterClass = "Warrior")
    {
        // Check character count limit (max 10)
        var existingCount = await GetCharacterCountAsync(accountId);
        if (existingCount >= 10)
        {
            Logger.Log(LogLevel.Debug, "CHARACTER",$"Create failed: Account {accountId} already has 10 characters");
            return null;
        }
        
        var sql = @"
            INSERT INTO characters (account_id, name, class, created_at)
            VALUES (@AccountId, @Name, @Class, @CreatedAt)
            RETURNING *";
        
        using var connection = await _db.GetConnectionAsync();
        
        try
        {
            var character = await connection.QuerySingleAsync<Character>(sql, new
            {
                AccountId = accountId,
                Name = name,
                Class = characterClass,
                CreatedAt = DateTime.UtcNow
            });
            
            Logger.Log(LogLevel.Debug, "CHARACTER",$" Created: {name} (ID: {character.Id}) for account {accountId}");
            return character;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") // Unique violation
        {
            Logger.Log(LogLevel.Debug, "CHARACTER",$"Create failed: Name '{name}' already exists");
            return null;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23514") // Check violation
        {
            Logger.Log(LogLevel.Debug, "CHARACTER",$"Create failed: Invalid character name '{name}'");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "CHARACTER",$"Create failed: {ex.Message}");
            return null;
        }
    }
    

    /// Soft delete character
    public async Task<bool> DeleteCharacterAsync(int characterId, int accountId)
    {
        var sql = @"
            UPDATE characters 
            SET deleted_at = @DeletedAt 
            WHERE id = @CharacterId 
              AND account_id = @AccountId 
              AND deleted_at IS NULL";
        
        using var connection = await _db.GetConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            DeletedAt = DateTime.UtcNow,
            CharacterId = characterId,
            AccountId = accountId
        });
        
        if (rowsAffected > 0)
        {
            Logger.Log(LogLevel.Debug, "CHARACTER",$"Soft deleted: ID {characterId}");
            return true;
        }
        Logger.Log(LogLevel.Debug, "CHARACTER",$"Delete failed: Character {characterId} not found or already deleted");
        return false;
    }
    
    // Update character position
    public async Task UpdatePositionAsync(int characterId, int x, int y)
    {
        var sql = @"
            UPDATE characters 
            SET position_x = @X, position_y = @Y 
            WHERE id = @CharacterId";
        
        using var connection = await _db.GetConnectionAsync();
        await connection.ExecuteAsync(sql, new { X = x, Y = y, CharacterId = characterId });
    }
    
    /// Update last login timestamp
    public async Task UpdateLastLoginAsync(int characterId)
    {
        var sql = "UPDATE characters SET last_login = @Now WHERE id = @CharacterId";
        
        using var connection = await _db.GetConnectionAsync();
        await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow, CharacterId = characterId });
    }
    

    /// Get character count for an account
    private async Task<int> GetCharacterCountAsync(int accountId)
    {
        var sql = "SELECT COUNT(*) FROM characters WHERE account_id = @AccountId AND deleted_at IS NULL";
        
        using var connection = await _db.GetConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { AccountId = accountId });
    }
}
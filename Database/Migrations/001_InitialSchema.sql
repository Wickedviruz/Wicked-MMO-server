-- ==========================================
-- WICKED MMORPG - Initial Database Schema
-- ==========================================

-- Accounts table
CREATE TABLE IF NOT EXISTS accounts (
    id SERIAL PRIMARY KEY,
    username VARCHAR(32) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP,
    
    CONSTRAINT username_length CHECK (char_length(username) >= 3)
);

CREATE INDEX IF NOT EXISTS idx_accounts_username ON accounts(username);

-- Characters table
CREATE TABLE IF NOT EXISTS characters (
    id SERIAL PRIMARY KEY,
    account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    name VARCHAR(20) UNIQUE NOT NULL,
    class VARCHAR(20) NOT NULL DEFAULT 'Warrior',
    level INTEGER NOT NULL DEFAULT 1,
    experience BIGINT NOT NULL DEFAULT 0,
    
    -- Stats
    health INTEGER NOT NULL DEFAULT 100,
    max_health INTEGER NOT NULL DEFAULT 100,
    mana INTEGER NOT NULL DEFAULT 50,
    max_mana INTEGER NOT NULL DEFAULT 50,
    
    -- Position
    position_x INTEGER NOT NULL DEFAULT 0,
    position_y INTEGER NOT NULL DEFAULT 0,
    
    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP,
    
    -- Soft delete (30 days)
    deleted_at TIMESTAMP NULL,
    
    CONSTRAINT character_name_length CHECK (char_length(name) >= 3 AND char_length(name) <= 20),
    CONSTRAINT character_name_alpha CHECK (name ~ '^[a-zA-Z]+$')
);

CREATE INDEX IF NOT EXISTS idx_characters_account_id ON characters(account_id);
CREATE INDEX IF NOT EXISTS idx_characters_name ON characters(name);
CREATE INDEX IF NOT EXISTS idx_characters_deleted_at ON characters(deleted_at) WHERE deleted_at IS NULL;

-- Ensure max 10 active characters per account
-- (This is enforced in application logic, not DB constraint for now)

-- Test data (optional, remove in production)
-- INSERT INTO accounts (username, password_hash, email) 
-- VALUES ('testuser', '$2a$11$...', 'test@example.com');

COMMENT ON TABLE accounts IS 'User accounts for login';
COMMENT ON TABLE characters IS 'Player characters (max 10 per account, soft delete after 30 days)';
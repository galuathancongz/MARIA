const Database = require('better-sqlite3');
const path = require('path');

const dbPath = path.join(__dirname, 'maria.db');
const db = new Database(dbPath);

// Enable WAL mode for better concurrent read performance
db.pragma('journal_mode = WAL');

// Create tables
db.exec(`
  CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    created_at DATETIME DEFAULT (datetime('now')),
    last_login DATETIME
  );

  CREATE TABLE IF NOT EXISTS game_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER UNIQUE NOT NULL,
    level INTEGER DEFAULT 0,
    name_player TEXT DEFAULT 'username',
    age INTEGER DEFAULT 24,
    subject INTEGER DEFAULT 0,
    subject_name TEXT DEFAULT '',
    resources_json TEXT DEFAULT '{"resources":[]}',
    heart_json TEXT DEFAULT '{"valueHeart":5,"timeHeartCurrent":0,"timeHeartInfinite":0,"lastTimeEnd":0}',
    pack_json TEXT DEFAULT '{"listPack":[],"listShowPack":[]}',
    level2_json TEXT DEFAULT '{}',
    level3_json TEXT DEFAULT '{}',
    settings_json TEXT DEFAULT '{"sfxVolume":1.0,"musicVolume":1.0,"muteVibra":0}',
    updated_at DATETIME DEFAULT (datetime('now')),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
  );
`);

module.exports = db;

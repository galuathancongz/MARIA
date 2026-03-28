# MARIA Teaching Classroom - Server Setup Guide

## Tong Quan Kien Truc

```
[Unity WebGL Game] <-- REST API (JSON) --> [Node.js Server] <-- SQLite --> [maria.db]
       |                                         |
       |         Cung deploy len FTP              |
       +------ Server/public/ (WebGL build) ------+
```

---

## BUOC 1: Cai Dat Server (Local)

### Yeu cau
- **Node.js** >= 16.x (tai tu https://nodejs.org)
- Khong can cai SQLite rieng (better-sqlite3 da bao gom)

### Cai dat

```bash
# Mo terminal, di den folder Server
cd "D:\GithubUnity\MARIA Teaching Classroom\Server"

# Cai dat dependencies
npm install

# Chay server
node server.js
```

Server se chay tai `http://localhost:3000`

### Kiem tra

```bash
# Health check
curl http://localhost:3000/api/health

# Register user moi
curl -X POST http://localhost:3000/api/auth/register -H "Content-Type: application/json" -d "{\"username\":\"testuser\",\"password\":\"1234\"}"

# Login
curl -X POST http://localhost:3000/api/auth/login -H "Content-Type: application/json" -d "{\"username\":\"testuser\",\"password\":\"1234\"}"

# Save game data (thay TOKEN bang token nhan duoc tu login)
curl -X POST http://localhost:3000/api/gamedata/save -H "Content-Type: application/json" -H "Authorization: Bearer TOKEN" -d "{\"level\":5,\"namePlayer\":\"Test\",\"age\":25,\"subject\":1,\"subjectName\":\"Science\",\"resourcesJson\":\"{}\",\"heartJson\":\"{}\",\"packJson\":\"{}\",\"level2Json\":\"{}\",\"level3Json\":\"{}\",\"settingsJson\":\"{}\"}"

# Load game data
curl -H "Authorization: Bearer TOKEN" http://localhost:3000/api/gamedata/load
```

---

## BUOC 2: Setup Unity

### 2.1 Them cac Singleton vao Scene

Trong Unity Editor, mo scene `Game.unity`:

1. Tao 1 **Empty GameObject** ten `ServerManager`
2. Add 3 component vao `ServerManager`:
   - **ApiClient**
   - **AuthManager**
   - **SyncManager**

> Thu tu: ApiClient phai Init truoc AuthManager va SyncManager.
> Dat Script Execution Order: ApiClient (-100) < AuthManager (-50) < SyncManager (0)
> Hoac don gian: dat ApiClient la component dau tien.

### 2.2 Ket noi UI Login/Register

Tao UI man hinh Login (hoac them vao UIMainMenu). Vi du goi API:

```csharp
// DANG KY
AuthManager.Instance.Register("username", "password",
    onSuccess: (response) => {
        Debug.Log("Dang ky thanh cong! UserID: " + response.userId);
        // Chuyen sang MainMenu
    },
    onError: (error) => {
        Debug.LogError("Loi: " + error);
        // Hien thong bao loi
    }
);

// DANG NHAP
AuthManager.Instance.Login("username", "password",
    onSuccess: (response) => {
        Debug.Log("Dang nhap thanh cong!");
        // SyncManager tu dong LoadFromServer() sau khi login
        // Chuyen sang MainMenu
    },
    onError: (error) => {
        Debug.LogError("Loi: " + error);
    }
);

// DANG XUAT
AuthManager.Instance.Logout(onSuccess: () => {
    Debug.Log("Da dang xuat");
    // Quay ve man hinh Login
});

// KIEM TRA TRANG THAI
if (AuthManager.Instance.IsLoggedIn) {
    // Da dang nhap
    string username = AuthManager.Instance.CurrentUsername;
}
```

### 2.3 Dong Bo Data Thu Cong (Neu can)

```csharp
// Save len server bat ky luc nao
SyncManager.Instance.SaveToServer(() => {
    Debug.Log("Save xong!");
});

// Load tu server
SyncManager.Instance.LoadFromServer(() => {
    Debug.Log("Load xong!");
});
```

### 2.4 Tu Dong Sync

SyncManager da tu dong:
- **Load tu server** khi login thanh cong
- **Save len server** khi app mat focus (nguoi choi tab out / minimize)
- **Save len server** khi thang game (ClassicMode.OnWinGame)

---

## BUOC 3: Deploy Len FTP (Production)

### 3.1 Build WebGL trong Unity

1. File > Build Settings > Platform: **WebGL**
2. Build ra 1 folder (vi du: `WebGLBuild/`)

### 3.2 Chuan bi Server cho deploy

```
Server/
├── server.js
├── database.js
├── package.json
├── routes/
│   ├── auth.js
│   └── gamedata.js
├── middleware/
│   └── auth.js
└── public/              <-- DAT WEBGL BUILD O DAY
    ├── index.html
    ├── Build/
    │   ├── game.data
    │   ├── game.framework.js
    │   ├── game.loader.js
    │   └── game.wasm
    └── TemplateData/
```

### 3.3 Upload len FTP

1. Copy **toan bo folder `Server/`** len FTP server
2. Copy **noi dung WebGL build** vao `Server/public/`
3. SSH vao server va chay:

```bash
cd /path/to/Server
npm install
# Chay voi PM2 (khuyen nghi)
npm install -g pm2
pm2 start server.js --name maria-server

# Hoac chay truc tiep
node server.js
```

### 3.4 Cau hinh port va domain

**Neu server chay tren port 3000:**
- Cau hinh Nginx/Apache reverse proxy de tro domain den port 3000
- Hoac doi PORT: `PORT=80 node server.js`

**Env variables:**
```bash
# Doi JWT secret cho bao mat (QUAN TRONG!)
export JWT_SECRET="your-super-secret-key-here"
export PORT=3000
```

### 3.5 Nginx Config (Vi du)

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

---

## BUOC 4: Unity Build Config cho Production

Trong file `ApiClient.cs`, URL da tu dong xu ly:

```csharp
#if UNITY_EDITOR
    private const string BASE_URL = "http://localhost:3000";  // Local dev
#else
    private const string BASE_URL = "";  // Production: relative URL (cung domain)
#endif
```

- **Editor/Local:** Goi `http://localhost:3000/api/...`
- **WebGL Build:** Goi `/api/...` (relative, cung domain voi game)

> Neu server o domain khac, thay `BASE_URL` trong `#else` block.

---

## API Reference

### Authentication

| Endpoint | Method | Body | Response | Auth |
|----------|--------|------|----------|------|
| `/api/auth/register` | POST | `{username, password}` | `{success, token, userId, username}` | No |
| `/api/auth/login` | POST | `{username, password}` | `{success, token, userId, username}` | No |
| `/api/auth/logout` | POST | - | `{success, message}` | Yes |
| `/api/auth/me` | GET | - | `{success, user}` | Yes |

### Game Data

| Endpoint | Method | Body | Response | Auth |
|----------|--------|------|----------|------|
| `/api/gamedata/save` | POST | GameDataSaveRequest | `{success, message}` | Yes |
| `/api/gamedata/load` | GET | - | `{success, data: GameDataPayload}` | Yes |

### Health

| Endpoint | Method | Response |
|----------|--------|----------|
| `/api/health` | GET | `{status, timestamp}` |

---

## Database Schema

### users
```sql
CREATE TABLE users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,        -- bcrypt hash
    created_at DATETIME DEFAULT NOW,
    last_login DATETIME
);
```

### game_data
```sql
CREATE TABLE game_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER UNIQUE NOT NULL,    -- 1 user = 1 row
    level INTEGER DEFAULT 0,
    name_player TEXT DEFAULT 'username',
    age INTEGER DEFAULT 24,
    subject INTEGER DEFAULT 0,          -- ESubject enum
    subject_name TEXT DEFAULT '',
    resources_json TEXT,                 -- PlayerResources JSON
    heart_json TEXT,                     -- DataHeart JSON
    pack_json TEXT,                      -- GamePackData JSON
    level2_json TEXT,                    -- Level2Data JSON
    level3_json TEXT,                    -- Level3Data JSON
    settings_json TEXT,                  -- {sfxVolume, musicVolume, muteVibra}
    updated_at DATETIME,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
```

---

## Danh Sach File Da Tao/Sua

### File Moi (Server - Ngoai Assets)
| File | Mo ta |
|------|-------|
| `Server/package.json` | Dependencies |
| `Server/server.js` | Entry point Express |
| `Server/database.js` | SQLite schema |
| `Server/middleware/auth.js` | JWT middleware |
| `Server/routes/auth.js` | Register/Login/Logout API |
| `Server/routes/gamedata.js` | Save/Load game data API |
| `Server/.gitignore` | Ignore node_modules, db |

### File Moi (Unity - Trong Assets)
| File | Mo ta |
|------|-------|
| `Assets/_GameLuzart/ServerAPI/Script/ApiClient.cs` | HTTP client (UnityWebRequest) |
| `Assets/_GameLuzart/ServerAPI/Script/AuthManager.cs` | Login/Register/Logout |
| `Assets/_GameLuzart/ServerAPI/Script/SyncManager.cs` | Dong bo data len/xuong server |
| `Assets/_GameLuzart/ServerAPI/Script/ApiModels.cs` | Request/Response models |

### File Da Sua
| File | Thay doi |
|------|----------|
| `Assets/_GameLuzart/GameManager/Script/Manager/GameMode/ClassicMode.cs` | Them SyncManager.SaveToServer() khi win |

---

## Troubleshooting

### Server khong chay duoc
- Kiem tra Node.js da cai: `node --version`
- Kiem tra port 3000 co bi dung: `netstat -ano | findstr :3000`
- Xem log loi khi chay `node server.js`

### Unity khong ket noi duoc server
- Kiem tra server dang chay (`http://localhost:3000/api/health`)
- Kiem tra Console Unity co loi CORS khong
- WebGL build phai cung domain voi server (hoac cau hinh CORS)

### Data khong dong bo
- Kiem tra `AuthManager.Instance.IsLoggedIn` = true
- Xem Console Unity co log `[SyncManager]` khong
- Test API bang curl/Postman truoc

### Quen mat khau
- Hien tai chua co reset password. Co the xoa user truc tiep trong SQLite:
```bash
# Mo database
sqlite3 Server/maria.db
# Xoa user
DELETE FROM users WHERE username = 'ten_user';
```

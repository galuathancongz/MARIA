namespace Luzart
{
    [System.Serializable]
    public class ApiResponse
    {
        public bool success;
        public string message;
    }

    // ========== AUTH ==========
    [System.Serializable]
    public class AuthRequest
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public bool success;
        public string message;
        public string token;
        public int userId;
        public string username;
    }

    // ========== GAME DATA ==========
    [System.Serializable]
    public class GameDataSaveRequest
    {
        public int level;
        public string namePlayer;
        public int age;
        public int subject;
        public string subjectName;
        public string resourcesJson;
        public string heartJson;
        public string packJson;
        public string level2Json;
        public string level3Json;
        public string settingsJson;
    }

    [System.Serializable]
    public class GameDataLoadResponse
    {
        public bool success;
        public string message;
        public GameDataPayload data;
    }

    [System.Serializable]
    public class GameDataPayload
    {
        public int level;
        public string namePlayer;
        public int age;
        public int subject;
        public string subjectName;
        public string resourcesJson;
        public string heartJson;
        public string packJson;
        public string level2Json;
        public string level3Json;
        public string settingsJson;
        public string updatedAt;
    }

    [System.Serializable]
    public class SettingsData
    {
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public int muteVibra = 0;
    }
}

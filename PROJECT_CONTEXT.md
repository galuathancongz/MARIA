# MARIA Teaching Classroom - Project Context

## Tong Quan
- **Ten du an:** MARIA Teaching Classroom
- **Cong ty:** Luzart
- **Phien ban:** 1.2.6
- **Loai:** Game giao duc / day hoc AI, build WebGL de deploy len server
- **Platform chinh:** WebGL (1280x720), ho tro Standalone (1920x1080)
- **Orientation:** LandscapeLeft
- **Color Space:** Linear
- **Bundle ID:** com.DefaultCompany.2DProject

---

## Cau Truc Scene
| Scene | Duong dan | Muc dich |
|-------|-----------|----------|
| Game.unity | `Assets/_GameLuzart/GameManager/Scenes/Game.unity` | Scene chinh, entry point duy nhat |
| UIEnvironment.unity | `Assets/_GameLuzart/GameManager/Scenes/UIEnvironment.unity` | Quan ly UI |

Chi co Game.unity trong Build Settings (index 0).

---

## Kien Truc He Thong

### 1. GameManager (Singleton)
- **File:** `Assets/_GameLuzart/GameManager/Script/Manager/GameManager.cs`
- Quan ly game state: `EGameState { None, Playing, Finish }`
- Quan ly game mode: `EGameMode { None, Classic }`
- Dieu phoi flow: `PlayGameMode()`, `BackToMain()`, `Restart()`, `StartLevel()`
- Static flags: `IS_TEST`, `IS_REMOVE_ADS_REWARD`, `IS_REMOVE_ADS_INTER`

### 2. GameCoordinator (Event Manager)
- `ActionOnLoadDoneLevel` - Event khi load level xong
- `ActionOnEndGame` - Event khi game ket thuc (win/lose)

### 3. DataManager (Save/Load du lieu nguoi choi)
- **File:** `Assets/_GameLuzart/GameManager/Script/Manager/DataManager.cs`
- Ke thua `SingletonSaveLoad<GameData>`
- Du lieu luu:
  - `level` (int) - Level hien tai
  - `namePlayer` (string) - Ten nguoi choi
  - `age` (int) - Tuoi
  - `subject` (ESubject enum) - Mon hoc dang chon
  - `subjectName` (string) - Ten mon hoc
- Luu qua **PlayerPrefs** voi key `"key_gamedata"`
- Tu dong save khi `OnNewDay`

### 4. GameRes (Quan ly tai nguyen)
- **File:** Quan ly `PlayerResources` bang JSON serialization
- PlayerPrefs key: `"PlayerResources"`
- Cac method: `isAddRes()`, `GetRes()`, `AddRes()`

---

## He Thong Save/Load

### SaveLoadUtil.cs
- **File:** `Assets/_GameLuzart/Utility/Script/Other/SaveLoadUtil.cs`

| Phuong thuc | Mo ta |
|-------------|-------|
| `SaveDataPrefs<T>()` / `LoadDataPrefs<T>()` | Luu/doc qua PlayerPrefs |
| `SerializeObjectToFile<T>()` / `DeserializeObjectFromFile<T>()` | Luu/doc file co ma hoa |
| `DeserializeObjectFromFileAsync<T>()` | Doc file async |
| `ByteToFile()` / `ByteFromFile()` | Doc/ghi byte-level |

### Ma Hoa
- **AES-256** voi PBKDF2 key derivation
- Salt: `"luzart"`, Password: `"luzart"`
- Mode: AES_CBC_PKCS7, 1000 iterations
- `TypeSave { None, Encryption }`

### SingletonSaveLoad<TData, T>
- Base class chung cho cac manager can save/load
- Tu dong load khi Awake
- Tu dong save khi app mat focus / quay lai

---

## He Thong UI

### UIManager (Singleton)
- **File:** `Assets/_GameLuzart/Utility/Script/UIBase/UIManager.cs`
- 5 canvas layer (0-4) phan tang hien thi:
  - **Layer 0:** Gameplay chinh, shop, menu
  - **Layer 1:** Man hinh chung
  - **Layer 2:** Overlay, login, profile
  - **Layer 3:** Loading, notification
  - **Layer 4:** Toast, notification (cao nhat)

### UIBase (Base class)
- **File:** `Assets/_GameLuzart/Utility/Script/UIBase/UIBase.cs`
- Properties: `uiName`, `closeBtn`, `isAnimBtnClose`, `isCache`
- Methods: `Show()`, `Hide()`, `Setup()`, `RefreshUI()`, `OnAnimHideDone()`

### Danh sach UIName (50+ man hinh)

**Tutorial:** Tut1 → Tut6, Tut5_1

**Level 1:** Level1_0 → Level1_6
- Nested: Level1_1_1-3, Level1_2_1-4, Level1_3_1-6, Level1_4_1, Level1_5, Level1_6

**Level 2:** Level2_1 → Level2_6
- Nested: Level2_1_1, Level2_2_1, Level2_3_1

**Level 3:** Level3_1 → Level3_7
- Nested: Level3_1_1

**Level 4:** Level4_1 → Level4_5
- Nested: Level4_1_1-3, Level4_2_1-2, Level4_3_1-3, Level4_4_1, Level4_5

**Cac man hinh khac:** MainMenu, Gameplay, Settings, WinClassic, LoseClassic, Splash, LoadScene, Toast, Noti

### Cac UI Screen chinh
| Script | Chuc nang |
|--------|-----------|
| UIMainMenu.cs | Chon level, unlock theo tien do |
| UIGameplay.cs | Man hinh gameplay |
| UISettings.cs | Cai dat am thanh |
| UISplash.cs | Splash screen |
| UILoad.cs | Man hinh loading |
| UINoti.cs | Thong bao |
| UITop.cs | Thanh tren cung |

---

## He Thong Game Mode

### BaseMode (Abstract)
- Virtual methods: `StartLevel()`, `OnEndGame()`, `OnWinGame()`, `OnLoseGame()`, `PauseGame()`, `ResumeGame()`

### ClassicMode (Mode chinh)
- **File:** `Assets/_GameLuzart/GameManager/Script/Manager/GameMode/ClassicMode.cs`
- Load tu Resources: `GameMode/ClassicMode` (Prefab)
- Thoi gian: 20s khoi tao, 150s mac dinh
- Khi thang: Tang `DataManager.Instance.Data.level` va save

---

## He Thong Tai Nguyen & Economy

### RES_type (Enum)
| Type | ID | Mo ta |
|------|----|-------|
| Gold | 1 | Tien te chinh |
| Heart | 2 | Mang/luot choi |
| HeartTime | 3 | Tim theo thoi gian |
| Booster | 4 | Booster (Scale, ShuffleTool, VIP, Sort) |
| Chest | 5 | Ruong thuong |
| Spin | 6 | Vong quay |

### DataWrapperGame.cs (Utility)
- `ReceiveReward()` - Cong tai nguyen + log Firebase
- `ReceiveRewardShowPopUp()` - Cong tai nguyen + hien popup
- `SubtractResources()` - Tru tai nguyen (kiem tra du truoc)
- `GetResource()`, `ChangeResourceAmount()`

---

## He Thong Heart/Stamina

### HeartManager.cs
- **File:** `Assets/_GameLuzart/HeartManager/Script/HeartManager.cs`
- Max heart: **5** (standard), **8** (voi BattlePass VIP)
- Cooldown: **900 giay (15 phut)** moi heart
- Infinite heart: 900 giay duration
- `IsCanPlayNewGame` - Kiem tra co du heart khong
- `UseHeart()` - Tru heart
- Tu dong save khi app focus change
- Tinh delta-time khi app resume

### DataHeart (Serializable)
- `CountHeart` - So heart hien tai (0-5)
- `timeHeartCurrent` - Dem nguoc den heart tiep
- `timeHeartInfinite` - Thoi gian heart vo han
- `lastTimeEnd` - Timestamp luu cuoi
- `EStateHeart { None, Infinite }`

---

## He Thong Shop/Pack

### PackManager.cs (Singleton)
- **File:** `Assets/_GameLuzart/PackManager/Script/PackManager.cs`
- `DBPackSO` ScriptableObject database
- `GamePackData` JSON serialization (key: `"gamepackdata"`)
- Hien tai cac pack dang **commented out**
- Methods: `GetDBPack()`, `IsHasDBPack()`, `IsHasBuyPack()`

### DBPackSO.cs (ScriptableObject)
- **Asset:** `Assets/_GameLuzart/PackManager/Resources/DB_PackSO.asset`
- Fields: `productId`, `ePack`, `maxBuy`

---

## He Thong Observer/Event

### Observer.cs
- **File:** `Assets/_GameLuzart/Utility/Script/Other/Observer.cs`
- Pattern: Publisher-Subscriber
- Methods: `AddObserver()`, `RemoveObserver()`, `Notify()`

### ObserverKey (Cac event chinh)
| Key | Mo ta |
|-----|-------|
| TimeActionPerSecond | Tick moi giay |
| CoinObserverNormal | Event coin |
| CoinObserverTextRun | Coin text chay |
| OnNewDay | Reset ngay moi |
| QuestKey | Tien trinh quest |
| OnCompleteStage | Hoan thanh stage |
| OnTutorial | Event tutorial |
| OnChangeAmountBooster | Cap nhat booster |
| BlockRaycast | Block UI |
| PersonaDataChange | Thay doi data nguoi choi |

---

## He Thong Am Thanh

### AudioManager.cs
- Dieu chinh volume Music va SFX (luu PlayerPrefs)
- Toggle mute: Music, SFX, Vibration
- Nhac nen loop
- Hieu ung click khi nhan chuot

---

## He Thong Utility

### Singleton<T> (Thread-safe)
- **File:** `Assets/_GameLuzart/Utility/Script/Other/GameUtil.cs`
- Lock-based thread safety, auto-create instance

### GameUtil.cs
- Button effects: `ButtonOnClick()`, `OnClickAnim()`
- Text coloring: `StringColor()`
- Per-second timer + Observer notify
- Day counting: Auto detect ngay moi

### TimeUtils.cs
- `GetLongTimeCurrent` - Unix timestamp hien tai

---

## WebGL Support

### Package: `Assets/WebGLSupport/`
| File | Chuc nang |
|------|-----------|
| WebGLInput.cs | Input plugin voi P/Invoke sang JavaScript |
| WebGLInputMobile.cs | Input toi uu cho mobile |
| WebGLInputMobile.jslib | JavaScript interop |
| WebGLWindow.cs | Quan ly window |
| WebGLInputManipulator.cs | UIToolkit integration |
| WebGLUIToolkitTextField.cs | Text field wrapper |

### DLL Imports chinh:
- `WebGLInputInit()`, `WebGLInputCreate()`, `WebGLInputFocus()`
- `WebGLInputForceBlur()`, `WebGLInputOnValueChange()`, `WebGLInputSetSelectionRange()`

---

## Package Dependencies

| Package | Version | Muc dich |
|---------|---------|----------|
| com.unity.textmeshpro | 3.0.7 | Render text UI |
| com.unity.ugui | 1.0.0 | UI framework |
| com.unity.timeline | 1.7.7 | Animation timeline |
| com.unity.visualscripting | 1.9.4 | Visual scripting |
| com.unity.nuget.newtonsoft-json | 3.2.2 | JSON serialization |
| com.coffee.ui-particle | GitHub | Particle cho UI |
| com.unity.test-framework | 1.1.33 | Testing |

---

## Thu Vien Third-Party

| Thu vien | Duong dan | Muc dich |
|----------|-----------|----------|
| DOTween | `Assets/Plugins/Demigiant/DOTween/` | Animation tweening |
| Epic Toon FX | `Assets/Epic Toon FX/` | VFX library |
| GUI Sci-Fi | `Assets/GUI_Sci_FI/` | UI demo |
| TextMesh Pro | `Assets/TextMesh Pro/` | Text rendering |

---

## Cau Truc Level

Game co **4 Level chinh**, moi level co nhieu sub-level:

```
Tutorial (Tut1-6)
│
Level 1 (7 sub: 1_0 → 1_6)
├── Level1_1 → 1_1_1, 1_1_2, 1_1_3
├── Level1_2 → 1_2_1, 1_2_2, 1_2_3, 1_2_4
├── Level1_3 → 1_3_1, 1_3_2, 1_3_3, 1_3_4, 1_3_5, 1_3_6
├── Level1_4 → 1_4_1
├── Level1_5
└── Level1_6
│
Level 2 (6 sub: 2_1 → 2_6)
├── Level2_1 → 2_1_1
├── Level2_2 → 2_2_1
└── Level2_3 → 2_3_1
│
Level 3 (7 sub: 3_1 → 3_7)
└── Level3_1 → 3_1_1
│
Level 4 (5 sub: 4_1 → 4_5)
├── Level4_1 → 4_1_1, 4_1_2, 4_1_3
├── Level4_2 → 4_2_1, 4_2_2
├── Level4_3 → 4_3_1, 4_3_2, 4_3_3
├── Level4_4 → 4_4_1
└── Level4_5
```

---

## Cac File Quan Trong

| File | Duong dan |
|------|-----------|
| GameManager.cs | `Assets/_GameLuzart/GameManager/Script/Manager/GameManager.cs` |
| DataManager.cs | `Assets/_GameLuzart/GameManager/Script/Manager/DataManager.cs` |
| ClassicMode.cs | `Assets/_GameLuzart/GameManager/Script/Manager/GameMode/ClassicMode.cs` |
| UIManager.cs | `Assets/_GameLuzart/Utility/Script/UIBase/UIManager.cs` |
| UIBase.cs | `Assets/_GameLuzart/Utility/Script/UIBase/UIBase.cs` |
| SaveLoadUtil.cs | `Assets/_GameLuzart/Utility/Script/Other/SaveLoadUtil.cs` |
| HeartManager.cs | `Assets/_GameLuzart/HeartManager/Script/HeartManager.cs` |
| PackManager.cs | `Assets/_GameLuzart/PackManager/Script/PackManager.cs` |
| Observer.cs | `Assets/_GameLuzart/Utility/Script/Other/Observer.cs` |
| GameUtil.cs | `Assets/_GameLuzart/Utility/Script/Other/GameUtil.cs` |
| AudioManager.cs | `Assets/_GameLuzart/Utility/Script/Other/AudioManager.cs` |
| WebGLInput.cs | `Assets/WebGLSupport/WebGLInput/WebGLInput.cs` |
| DB_PackSO.asset | `Assets/_GameLuzart/PackManager/Resources/DB_PackSO.asset` |

---

## Enum Tong Hop

| Enum | Gia tri | File/Context |
|------|---------|--------------|
| EGameState | None, Playing, Finish | GameManager |
| EGameMode | None, Classic | GameManager |
| EStateHeart | None, Infinite | HeartManager |
| RES_type | Gold(1), Heart(2), HeartTime(3), Booster(4), Chest(5), Spin(6) | Resource system |
| Difficulty | Normal, Hard, SuperHard | Game difficulty |
| EStateClaim | CanClaimDontClaimed, CanClaim, Claimed, WillClaim, NeedIAP, Chest | Reward system |
| UIName | 50+ values | UIManager |
| ESubject | (teaching subjects) | DataManager |

---

## Tom Tat Flow Game

1. **Khoi dong:** Game.unity load → GameManager init → UISplash hien thi
2. **Menu:** UIMainMenu hien thi cac level da unlock
3. **Chon level:** GameManager.StartLevel() → Load ClassicMode prefab
4. **Gameplay:** ClassicMode chay timer (150s), nguoi choi hoc/lam bai
5. **Ket thuc:**
   - **Thang:** Tang level, save data, hien UIWinClassic
   - **Thua:** Hien UILoseClassic, tru heart
6. **Heart:** Moi luot choi tieu 1 heart, hoi phuc 15 phut/heart, max 5
7. **Save:** Tu dong save qua PlayerPrefs khi chuyen focus, doi ngay

---

*Generated: 2026-03-29*

namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class GameManager : Singleton<GameManager>
    {
    
        public static bool IS_TEST = false;
        public static bool IS_REMOVE_ADS_REWARD = false;
        public static bool IS_REMOVE_ADS_INTER = false;
    
        public Transform parentSpawn;
        public GameCoordinator gameCoordinator;
        public BaseMode currentMode { get; private set; }
        public EGameMode ECurrentMode { get; private set; }
        public static string PATH_MODE_CLASSIC = "GameMode/ClassicMode";
        private Dictionary<EGameMode, string> dictPathMode = new Dictionary<EGameMode, string>()
        {
            {EGameMode.Classic,PATH_MODE_CLASSIC },
        };
        private ClassicMode classicMode;
        public EGameState EGameStatus;
        private void Start()
        {
            StartGame();
        }
        private void StartGame()
        {
            Application.targetFrameRate = 60;
            UIManager.Instance.ShowUI(UIName.Splash);
            LoadReferent();
            DataManager.Instance.Initialize();
        }
    
        private void LoadReferent()
        {
            if (parentSpawn == null)
            {
                parentSpawn = UIManager.Instance.transform.GetChild(0);
            }
            if (gameCoordinator == null)
            {
                gameCoordinator = GameObject.FindFirstObjectByType<GameCoordinator>();
            }
        }
        public void BackToMain()
        {
            DisableCurrentMode();
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }
        public void Restart()
        {
            DisableCurrentMode();
            UIManager.Instance.HideAll();
            PlayGameMode(EGameMode.Classic, DataManager.Instance.CurrentLevel);
        }
        public void PlayGameMode(EGameMode eGameMode, int level)
        {
            DisableCurrentMode();
            switch (eGameMode)
            {
                case EGameMode.Classic:
                    {
                        classicMode = InitMode(EGameMode.Classic, classicMode) as ClassicMode;
                        break;
                    }
            }
            ECurrentMode = eGameMode;
            StartLevel(level);
        }
    
        private BaseMode InitMode(EGameMode eGameMode, BaseMode baseModeLoad)
        {
            if (ECurrentMode != eGameMode)
            {
                ECurrentMode = eGameMode;
                if (baseModeLoad == null)
                {
                    BaseMode baseMode = Resources.Load<BaseMode>(dictPathMode[eGameMode]);
                    baseModeLoad = Instantiate(baseMode, parentSpawn);
                    currentMode = baseModeLoad;
                }
                else
                {
                    currentMode = baseModeLoad;
                }
            }
            currentMode.gameObject.SetActive(true);
            return baseModeLoad;
        }
        private void StartLevel(int level)
        {
            UIManager.Instance.HideAllUIIgnore(UIName.LoadScene);
            currentMode.StartLevel(level);
            SetGameStatus(EGameState.Playing);
        }
        public void SetGameStatus(EGameState eGameState)
        {
            EGameStatus = eGameState;
        }
        public void DisableCurrentMode()
        {
            if (currentMode != null)
            {
                if (EGameStatus == EGameState.Playing)
                {
                    currentMode.OnEndGame(false);
                }
                currentMode.gameObject.SetActive(false);
            }
        }
    
    
    
    }
    public enum EGameState
    {
        None = 0,
        Playing = 1,
        Finish = 2,
    }
    
    
}

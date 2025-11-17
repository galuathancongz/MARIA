namespace Luzart
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;

    public class UIManager : Singleton<UIManager>
    {
        private const string PATH_UI = "";
        //public UITop topUI;
        public Transform[] rootOb;
        public UIBase[] listSceneCache;

        public Canvas canvas;

        private List<UIBase> listScreenActive = new List<UIBase>();
        private Dictionary<UIName, UIBase> cacheScreen = new Dictionary<UIName, UIBase>();
        /// <summary>
        /// 0: UiController, shop, event, main, guild, free (on tab)
        /// 1: General (down top)
        /// 2: top, changeScene,login, ....
        /// 3: loading, NotiUpdate
        /// top: -1: not action || -2: Hide top || >=0: Show top
        /// </summary>
        private Dictionary<UIName, string> dir = new Dictionary<UIName, string>
    {
        // UIName, rootIdx,topIdx,loadPath
            {UIName.MainMenu,"0,0,UIMainMenu" },
            {UIName.Gameplay, "0,0,UIGameplay"},
            {UIName.Settings,"1,0,UISettings" },
            {UIName.WinClassic,"1,0,UIWinClassic" },
            {UIName.LoseClassic,"1,0,UILoseClassic" },
            {UIName.Splash,"0,0,UISplash" },
            {UIName.LoadScene,"3,0,UILoadScene" },
            {UIName.Toast,"4,0,UIToast" },
            {UIName.CoinSpawn,"2,0,UICoinSpawn" },
            {UIName.Rate,"2,0,UIRate" },
            {UIName.BattlePass,"1,0,UIBattlePass" },
            {UIName.TicketTally,"1,0,UITicketTally" },
            {UIName.AddCoin,"1,0,UIAddCoin" },
            {UIName.AddHeart,"1,0,UIAddHeart" },
            {UIName.TicketTallyNoti,"1,0,UITicketTallyNoti" },
            {UIName.AddBoosterFreeze,"1,0,UIBoosterFreeze" },
            {UIName.AddBoosterJump,"1,0,UIBoosterJump"},
            {UIName.Tips,"1,0,UITips" },
            {UIName.AddBoosterExpansion,"1,0,UIBoosterExpansion" },
            {UIName.UnlockFreeze,"1,0,UIUnlockFreeze" },
            {UIName.UnlockJump , "1,0,UIUnlockJump" },
            {UIName.UnlockExpansion,"1,0,UIUnlockExpansion" },
            {UIName.Pause,"1,0,UIPause" },
            {UIName.SecondChange,"1,0,UISecondChance" },
            {UIName.EventTicketGiveUp,"1,0,UIEventTicketGiveUp" },
            {UIName.EventTicketTimeOut,"1,0,UIEventTicketTimeOut" },
            {UIName.GiveUp,"1,0,UIGiveUp" },
            {UIName.ReceiveRewardEventTicket,"2,0,UIReceiveRewardEventTicket" },
            {UIName.BattlePassTutorial,"2,0,UIBattlePassTutorial" },
            {UIName.TutorialStepBattlePass,"2,0,UITutorialStepBattlePass" },
            {UIName.TutorialStepTicketTally,"2,0,UITutorialStepTicketTally" },
            {UIName.ReceiveRes,"2,0,UIReceiveRes" },
            {UIName.TutorialBooster0,"2,0,UITutorialTimeFreeze" },
            {UIName.TutorialBooster1,"2,0,UITutorialJump" },
            {UIName.TutorialBooster2,"2,0,UITutorialExpansion" },
            {UIName.SelectLevel,"2,0,UISelectLevel" },
            {UIName.AnimationItem,"2,0,UIAnimationItem" },
            {UIName.JourneyToSuccess,"2,0,UIJourneyToSuccess" },
            {UIName.TutorialStepJourneyToSuccess,"2,0,UITutorialStepJourneyToSuccess" },
            {UIName.FlightEndurance,"2,0,UIFlightEndurance" },
            {UIName.FlightEnduranceNoti,"2,0,UIFlightEnduranceNoti" },
            {UIName.FlightEnduranceTutorial,"3,0,UIFlightEnduranceTutorial" },
            {UIName.FlightEnduranceStart,"3,0,UIFlightEnduranceStart" },
            {UIName.FlightEnduranceReward,"3,0,UIFlightEnduranceReward" },
            {UIName.TutorialStepFlightEndurance,"3,0,UITutorialStepFlightEndurance" },

            {UIName.AreYouSureTimeOut,"1,0,UIAreYouSureTimeOut" },
            {UIName.AreYouSureGiveUp,"1,0,UIAreYouSureGiveUp" },

            {UIName.RiseOfKittens,"1,0,UIRiseOfKittens" },
            {UIName.Cup,"1,0,UICup" },
            {UIName.CupNoti,"1,0,UICupNoti" },
            {UIName.CupTutorial,"2,0,UICupTutorial" },
            {UIName.CupResult,"1,0,UICupResult" },
            {UIName.Profile,"1,0,UIProfile" },
            {UIName.DailyLogin, "1,0,UIDailyLogin" },
            {UIName.LuckySpin, "1,0,UILuckySpin" },
            {UIName.RiseOfKittenTutorial, "1,0,UIRiseOfKittenTutorial" },
            {UIName.Racing, "1,0,UIRacing" },
            {UIName.RacingTutorial, "1,0,UIRacingTutorial" },
            {UIName.RacingNoti, "1,0,UIRacingNoti" },
            {UIName.RacingLose, "1,0,UIRacingLose" },
            {UIName.RacingWin, "1,0,UIRacingWin" },
            {UIName.Noti, "1,0,UINoti" },
            


        // Pack
        {UIName.PackBattlePass, "2,0,UIPackBattlePass"},
        {UIName.PackTicketTally, "2,0,UIPackTicketTally"},
        {UIName.PackNoAds, "2,0,UIPackNoAds"},
        {UIName.PackStarter, "2,0,UIPackStarter"},
        {UIName.PackMini, "2,0,UIPackMini"},
        {UIName.PackLarge, "2,0,UIPackLarge"},
        {UIName.PackLifeAndCoin, "2,0,UIPackLifeAndCoin"},
        {UIName.PackSuper, "2,0,UIPackSuper"},
        {UIName.PackVIP, "2,0,UIPackVIP"},
        {UIName.PackValentine, "2,0,UIPackValentine"},
        {UIName.PackNoAdsBundle, "2,0,UIPackNoAdsBundle"},

    };

        private Dictionary<UIName, DataUIBase> dic2;

        public UIName CurrentName { get; private set; }
        public bool IsAction { get; set; }
        private void Awake()
        {
            dic2 = new Dictionary<UIName, DataUIBase>();
            foreach (var i in dir)
            {
                if (!dic2.ContainsKey(i.Key))
                {
                    var t = i.Value.Split(',');
                    dic2.Add(i.Key, new DataUIBase(int.Parse(t[0]), int.Parse(t[1]), t[2]));
                }
            }
            for (int i = 0; i < listSceneCache.Length; i++)
            {
                if (!cacheScreen.ContainsKey(listSceneCache[i].uiName))
                {
                    cacheScreen.Add(listSceneCache[i].uiName, listSceneCache[i]);
                }
            }
            //if (SdkUtil.isiPad())
            //{
            //    GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;
            //}
            //else
            //{
            //    GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
            //}
            IsAction = false;
        }
        public void ShowUI(UIName uIScreen, Action onHideDone = null)
        {
            ShowUI<UIBase>(uIScreen, onHideDone);
        }
        public T ShowUI<T>(UIName uIScreen, Action onHideDone = null) where T : UIBase
        {
            UIBase current = listScreenActive.Find(x => x.uiName == uIScreen);
            if (!current)
            {
                current = LoadUI(uIScreen);
                current.uiName = uIScreen;
                AddScreenActive(current, true);
            }
            current.transform.SetAsLastSibling();
            current.Show(onHideDone);
            CurrentName = uIScreen;
            return current as T;
        }
        public void ShowToast(string toast)
        {
            var ui = ShowUI<UIToast>(UIName.Toast);
            ui.Init(toast);
        }
        private void AddScreenActive(UIBase current, bool isTop)
        {
            var idx = listScreenActive.FindIndex(x => x.uiName == current.uiName);
            if (isTop)
            {
                if (idx >= 0)
                {
                    listScreenActive.RemoveAt(idx);
                }
                listScreenActive.Add(current);
            }
            else
            {
                if (idx < 0)
                {
                    listScreenActive.Add(current);
                }
            }
        }
        //public void LoadScene(Action onLoad, Action onDone, float timeLoad = 0.75f, float timeHide = 0.25f)
        //{
        //    UILoadScene uILoadScene = ShowUI<UILoadScene>(UIName.LoadScene);
        //    uILoadScene.LoadSceneCloud(onLoad, onDone, timeLoad, timeHide);
        //}

        private static Action actionRefreshUI = null;
        public static void AddActionRefreshUI(Action callBack)
        {
            actionRefreshUI += callBack;
        }
        public static void RemoveActionRefreshUI(Action callBack)
        {
            actionRefreshUI -= callBack;
        }
        public void RefreshUI()
        {
            var idx = 0;
            while (listScreenActive.Count > idx)
            {
                listScreenActive[idx].RefreshUI();
                idx++;
            }
            actionRefreshUI?.Invoke();
            //topUI.RefreshUI();
            //GameManager.OnRefreshUI?.Invoke();
        }

        //private UIToast _uiToast;
        //public UIToast UIToast()
        //{
        //    if (_uiToast == null)
        //    {
        //        _uiToast = GetComponentInChildren<UIToast>();
        //    }
        //    return _uiToast;
        //}

        public T GetUI<T>(UIName uIScreen) where T : UIBase
        {
            var c = LoadUI(uIScreen);
            return c as T;
        }

        public UIBase GetUI(UIName uIScreen)
        {
            return LoadUI(uIScreen);
        }

        public UIBase GetUiActive(UIName uIScreen)
        {
            return listScreenActive.Find(x => x.uiName == uIScreen);
        }

        public T GetUiActive<T>(UIName uIScreen) where T : UIBase
        {
            var ui = listScreenActive.Find(x => x.uiName == uIScreen);
            if (ui)
            {
                return ui as T;
            }
            else
            {
                return default;
            }
        }

        private UIBase LoadUI(UIName uIScreen)
        {
            UIBase current = null;
            if (cacheScreen.ContainsKey(uIScreen))
            {
                current = cacheScreen[uIScreen];
                if (current == null)
                {
                    var idx = dic2[uIScreen].rootIdx;
                    var pf = Resources.Load<UIBase>(PATH_UI + dic2[uIScreen].loadPath);
                    current = Instantiate(pf, rootOb[idx]);
                    cacheScreen[uIScreen] = current;
                }
            }
            else
            {
                var idx = dic2[uIScreen].rootIdx;
                var pf = Resources.Load<UIBase>(PATH_UI + dic2[uIScreen].loadPath);
                current = Instantiate(pf, rootOb[idx]);
                cacheScreen.Add(uIScreen, current);
            }
            return current;
        }

        public void RemoveActiveUI(UIName uiName)
        {
            var idx = listScreenActive.FindIndex(x => x.uiName == uiName);
            if (idx >= 0)
            {
                var ui = listScreenActive[idx];
                listScreenActive.RemoveAt(idx);
                if (!ui.isCache && cacheScreen.ContainsKey(uiName))
                {
                    cacheScreen[uiName] = null;
                }
            }
        }

        public void HideAllUIIgnore(UIName uiName = UIName.LoadScene)
        {
            int length = listScreenActive.Count;
            for (int i = 0; i < length; i++)
            {
                if (listScreenActive.Count == 0)
                {
                    continue;
                }
                HideUIIgnore(listScreenActive[0]);
            }
            void HideUIIgnore(UIBase uiBase)
            {
                if (uiBase.uiName != uiName)
                {
                    uiBase.Hide();
                }
            }
        }

        public void HideAll()
        {
            while (listScreenActive.Count > 0)
            {
                listScreenActive[0].Hide();
            }
            //topUI.Hide();
        }
        public void HideAllUiActive()
        {
            while (listScreenActive.Count > 0)
            {
                listScreenActive[0].Hide();
            }
        }

        public void HideAllUiActive(params UIName[] ignoreUI)
        {
            for (int i = 0; i < listScreenActive.Count; i++)
            {
                for (int j = 0; j < ignoreUI.Length; j++)
                {
                    if (listScreenActive[i].uiName != ignoreUI[j])
                    {
                        listScreenActive[i].Hide();
                    }
                }
            }
        }

        public void HideUiActive(UIName uiName)
        {
            var ui = listScreenActive.Find(x => x.uiName == uiName);
            if (ui)
            {
                ui.Hide();
            }
        }
        public void ShowToastInternet()
        {
            ShowToast(KeyToast.NoInternetLoadAds);
        }

        public UIBase GetLastUiActive()
        {
            if (listScreenActive.Count == 0) return null;
            return listScreenActive.Last();
        }

        //public void ShowCoinSpawn(Action onFirstTime = null, Action onLastTime = null, Transform target = null)
        //{
        //    var ui = ShowUI<UICoinSpawn>(UIName.CoinSpawn);
        //    ui.InitCoinSpawn(onFirstTime, onLastTime, target);
        //}


        //public MainMenu_Home GetUIMainMenuHome()
        //{
        //    UIMainMenu uiMainMenu = GetUiActive<UIMainMenu>(UIName.MainMenu);
        //    MainMenu_Home uiHome = null;
        //    if (uiMainMenu != null)
        //    {
        //        uiHome = uiMainMenu.GetUIScreenMainMenu(EMainMenu.Home) as MainMenu_Home;
        //    }
        //    return uiHome;
        //}
        //public void ShowDataCacheMain()
        //{
        //    var ui = GetUIMainMenuHome();
        //    if (ui != null)
        //    {
        //        ui.ShowDataCache();
        //    }
        //}
        //public static UIName ConvertPack(EPack ePack)
        //{
        //    switch(ePack)
        //    {
        //        case EPack.SuperPack:
        //            {
        //                return UIName.PackSuper;
        //            }
        //        case EPack.VIPPack:
        //            {
        //                return UIName.PackVIP;
        //            }
        //        case EPack.MiniPack:
        //            {
        //                return UIName.PackMini;
        //            }
        //        case EPack.LargePack:
        //            {
        //                return UIName.PackLarge;
        //            }
        //        case EPack.LifeAndCoinPack:
        //            {
        //                return UIName.PackLifeAndCoin;
        //            }
        //        case EPack.ValentinePack:
        //            {
        //                return UIName.PackValentine;
        //            }
        //        default:
        //            {
        //                return UIName.PackStarter;
        //            }
        //    }
        //}
    }

    public enum UIName
    {
        None = 0,
        Gameplay = 1,
        Settings = 2,
        MainMenu = 3,
        WinClassic = 4,
        LoseClassic = 5,
        Splash = 6,
        LoadScene = 7,
        CoinSpawn = 8,
        Toast = 9,
        Rate = 10,
        BattlePass = 11,
        TicketTally = 12,
        AddCoin = 13,
        AddHeart = 14,
        TicketTallyNoti = 15,


        AddBoosterFreeze = 16,
        AddBoosterJump = 17,
        AddBoosterExpansion = 18,

        Tips = 19,
        UnlockFreeze = 20,
        UnlockJump = 21,
        UnlockExpansion = 22,

        Pause = 23,
        SecondChange = 24,
        EventTicketGiveUp = 25,
        EventTicketTimeOut = 26,

        GiveUp = 27,
        ReceiveRewardEventTicket = 28,
        BattlePassTutorial = 29,
        TutorialStepBattlePass = 30,
        TutorialStepTicketTally = 31,
        ReceiveRes = 32,
        TutorialBooster0 = 33,
        TutorialBooster1 = 34,
        TutorialBooster2 = 35,
        SelectLevel = 36,
        AnimationItem = 37,
        JourneyToSuccess = 38,
        TutorialStepJourneyToSuccess = 39,

        FlightEndurance = 40,
        FlightEnduranceStart = 41,
        FlightEnduranceNoti = 42,
        FlightEnduranceTutorial = 43,
        FlightEnduranceReward = 44,

        AreYouSureTimeOut = 45,
        AreYouSureGiveUp = 46,

        RiseOfKittens = 47,
        TutorialStepFlightEndurance = 48,

        Cup = 49,
        CupNoti = 50,
        CupTutorial = 51,
        CupResult = 52,
        Profile = 53,
        DailyLogin = 54,
        LuckySpin = 55,
        RiseOfKittenTutorial = 56,
        Racing = 57,
        RacingTutorial = 58,
        RacingNoti = 59,
        RacingLose = 60,
        RacingWin = 61,
        Noti = 62,


        PackBattlePass = 1000,
        PackTicketTally = 1001,
        PackNoAds = 1002,
        PackStarter = 1003,
        PackMini = 1004,
        PackLarge = 1005,
        PackLifeAndCoin = 1006,
        PackSuper = 1007,
        PackVIP = 1008,
        PackValentine = 1009,
        PackNoAdsBundle = 1010,

    }
    public class DataUIBase
    {
        public int rootIdx;
        public int topIdx;
        public string loadPath;

        public DataUIBase(int rootIdx, int topIdx, string loadPath)
        {
            this.rootIdx = rootIdx;
            this.topIdx = topIdx;
            this.loadPath = loadPath;
        }
    }

}

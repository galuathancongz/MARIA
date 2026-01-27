namespace Luzart
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;
    using UnityEngine.UI;

    public class UIManager : Singleton<UIManager>
    {
        private const string PATH_UI = "";
        //public UITop topUI;
        public Transform[] rootOb;
        public UIBase[] listSceneCache;

        public Canvas canvas;
        public GraphicRaycaster graphicRaycaster;

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
            {UIName.Noti,"4,0,UINoti" },
            
            {UIName.Level1,"1,0,UILevel1" },
            {UIName.Level2,"1,0,UILevel2" },
            {UIName.Level3,"1,0,UILevel3" },
            {UIName.Tutorial,"1,0,UITutorial" },

            {UIName.Level1_1,"1,0,Level/Level1/UILevel1_1" },
            {UIName.Level1_1_1,"1,0,Level/Level1/UILevel1_1_1" },
            {UIName.Level1_1_2,"1,0,Level/Level1/UILevel1_1_2" },
            {UIName.Level1_1_3,"1,0,Level/Level1/UILevel1_1_3" },
            {UIName.Level1_2,"1,0,Level/Level1/UILevel1_2" },
            {UIName.Level1_2_1,"1,0,Level/Level1/UILevel1_2_1" },
            {UIName.Level1_3,"1,0,Level/Level1/UILevel1_3" },
            {UIName.Level1_3_1,"1,0,Level/Level1/UILevel1_3_1" },
            {UIName.Level1_4,"1,0,Level/Level1/UILevel1_4" },
            {UIName.Level1_4_1,"1,0,Level/Level1/UILevel1_4_1" },
            {UIName.Level1_5,"1,0,Level/Level1/UILevel1_5" },
            {UIName.Level2_1,"1,0,Level/Level2/UILevel2_1" },
            {UIName.Level2_1_1,"1,0,Level/Level2/UILevel2_1_1" },
            {UIName.Level2_2,"1,0,Level/Level2/UILevel2_2" },
            {UIName.Level2_2_1,"1,0,Level/Level2/UILevel2_2_1" },
            {UIName.Level2_3,"1,0,Level/Level2/UILevel2_3" },
            {UIName.Level2_3_1,"1,0,Level/Level2/UILevel2_3_1" },
            {UIName.Level2_4,"1,0,Level/Level2/UILevel2_4" },
            {UIName.Level2_5,"1,0,Level/Level2/UILevel2_5" },
            {UIName.Level3_1,"1,0,Level/Level3/UILevel3_1" },
            {UIName.Level3_2,"1,0,Level/Level3/UILevel3_2" },
            {UIName.Level3_3,"1,0,Level/Level3/UILevel3_3" },
            {UIName.Level3_4,"1,0,Level/Level3/UILevel3_4" },
            {UIName.Level3_5,"1,0,Level/Level3/UILevel3_5" },
            {UIName.Level3_6,"1,0,Level/Level3/UILevel3_6" },
            {UIName.Level3_7,"1,0,Level/Level3/UILevel3_7" },


    };
        private List<UIName> listScenario = new List<UIName>()
        {
            UIName.Level1_1,
            UIName.Level1_2,
            UIName.Level1_3,
            UIName.Level1_4,
            UIName.Level1_5,
            UIName.Level2_1,
            UIName.Level2_2,
            UIName.Level2_3,
            UIName.Level2_4,
            UIName.Level2_5,
            UIName.Level3_1,
            UIName.Level3_2,
            UIName.Level3_3,
            UIName.Level3_4,
            UIName.Level3_5,
            UIName.Level3_6,
            UIName.Level3_7,
        };

        public T ShowNextScenario<T>() where T : UIBase
        {
            var ui = GetUiActive(CurrentName);
            var idx = listScenario.FindIndex(x => x == CurrentName);
            var nextIdx = idx + 1;
            return ShowUI<T>(listScenario[nextIdx]);
        }

        private Dictionary<UIName, DataUIBase> dic2;

        public UIName CurrentName { get; private set; }
        public bool IsAction { get; set; }
        private void Awake()
        {
            canvas ??= GetComponent<Canvas>();
            graphicRaycaster ??= GetComponent<GraphicRaycaster>();
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
            Observer.Instance.AddObserver(ObserverKey.BlockRaycast,BlockRaycast);
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
        private void OnDestroy()
        {
            Observer.Instance.RemoveObserver(ObserverKey.BlockRaycast, BlockRaycast);
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

        private void BlockRaycast(object data = null)
        {
            if(data == null)
            {
                return;
            }
            bool isBlock = (bool)data;
            graphicRaycaster.enabled = !isBlock;
        }
        public void BlockRaycast(bool isBlock)
        {
                       graphicRaycaster.enabled = !isBlock;
        }
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
        Toast = 8,
        Noti = 9,
        ReceiveRes = 10,
        Level1 =11,
        Level2 = 12,
        Level3 = 13,
        Tutorial = 14,
        
        Level1_1 = 110,
        Level1_1_1 = 111,
        Level1_1_2 = 112,
        Level1_1_3 = 113,
        Level1_2 = 120,
        Level1_2_1 = 121,
        Level1_3 = 130,
        Level1_3_1 = 131,
        Level1_4 = 140,
        Level1_4_1 = 141,
        Level1_5 = 150,
        Level2_1 = 211,
        Level2_1_1 = 212,
        Level2_2 = 220,
        Level2_2_1 = 221,
        Level2_3 = 230,
        Level2_3_1 = 231,
        Level2_4 = 240,
        Level2_5 = 250,
        Level3_1 = 310,
        Level3_2 = 320,
        Level3_3 = 330,
        Level3_4 = 340,
        Level3_5 = 350,
        Level3_6 = 360,
        Level3_7 = 370,
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

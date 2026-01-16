namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class UIMainMenu : UIBase
    {
        public TMP_Text txtStartLevel;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            txtStartLevel.text = "Start Level " + DataManager.Instance.GameData.level;
        }
        public void OnClickStartLevel()
        {
            int level = DataManager.Instance.GameData.level;
            switch (level)
            {
                case 0:
                    {
                        UIManager.Instance.ShowUI(UIName.Tutorial);
                        break;
                    }
                case 1:
                    {
                        UIManager.Instance.ShowUI(UIName.Level1);
                        break;
                    }
                case 2:
                    {
                        UIManager.Instance.ShowUI(UIName.Level2);
                        break;
                    }
                case 3:
                    {
                        UIManager.Instance.ShowUI(UIName.Level3);
                        break;
                    }
            }
        }
        public void OnReplayTutorial()
        {

        }
        public void OnExploreHelpMenu()
        {
        }
    }
}

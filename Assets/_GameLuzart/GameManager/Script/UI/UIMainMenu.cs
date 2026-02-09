namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIMainMenu : UIBase
    {
        public GameObject btnLevel1;
        public GameObject btnLevel2;
        public GameObject btnLevel4;
        public TMP_Text txtStartLevel;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            txtStartLevel.text = "Start Level " + Mathf.Clamp(DataManager.Instance.GameData.level,0,3);
            int lvl = DataManager.Instance.GameData.level;

            btnLevel1.gameObject.SetActive(lvl >= 2);
            btnLevel2.gameObject.SetActive(lvl >= 3);
            btnLevel4.gameObject.SetActive(lvl >= 4);

        }
        public void OnClickLevel1()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.Level1);
        }
        public void OnClickLevel2()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.Level2);
            Level2Manager.IsSendStartIdeationLab = false;
        }
        public void OnClickLevel4()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.Level4);
        }
        public void OnClickStartLevel()
        {
            int level = DataManager.Instance.GameData.level;
            level = Mathf.Clamp(level, 0, 3);
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
                        Level2Manager.IsSendStartIdeationLab = false;
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

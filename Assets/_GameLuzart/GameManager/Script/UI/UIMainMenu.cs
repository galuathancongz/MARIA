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
        public TMP_Text txtStartLevel;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            txtStartLevel.text = "Start Level " + DataManager.Instance.GameData.level;
            btnLevel1.gameObject.SetActive(false);
            btnLevel2.gameObject.SetActive(false);
            if(DataManager.Instance.GameData.level >= 2)
            {
                btnLevel1.gameObject.SetActive(true);
            }
            if(DataManager.Instance.GameData.level >= 3)
            {
                btnLevel2.gameObject.SetActive(true);
            }

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

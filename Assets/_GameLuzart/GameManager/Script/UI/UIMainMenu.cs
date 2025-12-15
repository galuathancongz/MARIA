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
            txtStartLevel.text  = "Start Level " + DataManager.Instance.GameData.level;
        }
        public void OnClickStartLevel()
        {

        }
        public void OnReplayTutorial()
        {

        }
        public void OnExploreHelpMenu()
        {
        }
    }
}

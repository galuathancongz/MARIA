using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Scene5 : Storyboard
{
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        DataManager.Instance.Data.level = 1;
        DataManager.Instance.SaveGameData();
        Level2Manager.IsSendStartIdeationLab = false;
    }
    public void OnClickStartLevel1()
    {
        UIManager.Instance.ShowUI<UIBase>(UIName.Level1);
    }
    public void OnClickReplayTutorial()
    {
        UIManager.Instance.HideAll();
        UIManager.Instance.ShowScenario(UIName.Tut1);
    }
    public void ExploreHelpMenu()
    {
        
    }
}

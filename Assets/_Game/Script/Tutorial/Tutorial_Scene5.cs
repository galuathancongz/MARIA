using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Scene5 : MonoBehaviour
{
    public void OnClickStartLevel1()
    {
        UIManager.Instance.ShowUI<UIBase>(UIName.Level1);
    }
    public void OnClickReplayTutorial()
    {
        UIManager.Instance.HideUiActive(UIName.Splash);
        UIManager.Instance.ShowUI<UIBase>(UIName.Splash);
    }
    public void ExploreHelpMenu()
    {
        
    }
}

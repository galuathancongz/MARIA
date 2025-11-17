using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParentSpawnItemPack : MonoBehaviour
{
    public RectTransform parentSpawn;
    public Button btnNextPack;
    public Button btnPrePack;
    public BaseSelect bsHideBtn;

    private Action actionClickNextPack = null;
    private Action actionClickPrePack = null;

    private void Start()
    {
        GameUtil.ButtonOnClick(btnPrePack, OnClickPrePack);
        GameUtil.ButtonOnClick(btnNextPack, OnClickNextPack);
        parentSpawn.anchorMax = Vector2.one;
        parentSpawn.anchorMin = Vector2.zero;
        parentSpawn.offsetMax = Vector2.zero;
        parentSpawn.offsetMin = Vector2.zero;
    }
    public void OnClickNextPack()
    {
        actionClickNextPack?.Invoke();
    }
    public void OnClickPrePack()
    {
        actionClickPrePack?.Invoke();
    }
    public void Initialze(Action actionClickPrePack = null,Action actionClickNextPack = null)
    {
        this.actionClickPrePack = actionClickPrePack;
        this.actionClickNextPack = actionClickNextPack;
    }


}

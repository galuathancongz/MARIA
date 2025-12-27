using DG.Tweening;
using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadString : MonoBehaviour
{
    [SerializeField] private StepKey stepKey;
    [SerializeField]
    private string[] arrayStepKey;
    public void OnClickButton(int index)
    {
        string key = stepKey.ToString();
        PlayerPrefs.SetString(key, arrayStepKey[index]);
        PlayerPrefs.Save();
    }
    public void OnClickFake()
    {
        float random = UnityEngine.Random.Range(0.5f, 3f);
        UIManager.Instance.BlockRaycast(true);
        DOVirtual.DelayedCall(DOVirtual.EasedValue(0, 1, random, Ease.Linear), () =>
        {
            ShowQuotaExceededNoti();
        });
        

    }

    private void ShowQuotaExceededNoti()
    {
        UIManager.Instance.BlockRaycast(false);
        var ui = UIManager.Instance.ShowUI<UINoti>(UIName.Noti);
        ui.InitPopup($"Quota exceeded for model {APIManager.Instance.ModelName}. Please check your API usage limits in the Google Cloud Console.");
    }
}

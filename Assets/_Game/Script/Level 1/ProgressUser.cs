using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressUser : MonoBehaviour
{
    public List<ProgressBarUI> listProgressBar = new List<ProgressBarUI>();
    private void OnEnable()
    {
        Observer.Instance.AddObserver(ObserverKey.PersonaDataChange, OnChangeProgress);
        OnChangeProgress();
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.PersonaDataChange, OnChangeProgress);
    }
    
    private void OnChangeProgress(object data = null)
    {

        float maxDict = PersonaManager.Instance.Data.MaxPersonaPoint;
        float max = Mathf.Max(maxDict, 1);
        int length = listProgressBar.Count;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            EPersonaType eType = (EPersonaType)index;
            var progressBar = listProgressBar[i];
            float current = PersonaManager.Instance.Data.GetPersonaAmount(eType);
            float percentCurrent = current / max;
            progressBar.SetSliderCache(percentCurrent, 0.2f);
        }
    }
}

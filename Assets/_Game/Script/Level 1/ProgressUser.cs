using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressUser : MonoBehaviour
{
    public List<ProgressBarUI> listProgressBar = new List<ProgressBarUI>();
    private void Awake()
    {
        Observer.Instance.AddObserver(ObserverKey.PersonaDataChange, OnChangeProgress);
    }
    private void OnDestroy()
    {
        Observer.Instance.RemoveObserver(ObserverKey.PersonaDataChange, OnChangeProgress);
    }
    private void OnChangeProgress(object data)
    {
        float max = DataManager.Instance.GameData.personaData.MaxPersonaPoint;
        int length = listProgressBar.Count;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            EPersonaType eType = (EPersonaType)index;
            var progressBar = listProgressBar[i];
            float current = DataManager.Instance.GameData.personaData.GetPersonaAmount(eType);
            float percentCurrent = current / max;
            progressBar.SetSlider(percentCurrent, percentCurrent);
        }
    }
}

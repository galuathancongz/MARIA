using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProfile : UIBase
{
    public TMP_InputField txtSubject;
    public TMP_InputField txtName;
    public TMP_InputField txtAge;
    public BaseSelect bsPersona;
    public Button btnLogout;

    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        txtSubject.onEndEdit.RemoveAllListeners();
        txtSubject.onEndEdit.AddListener(SaveSubject);
        txtName.onEndEdit.RemoveAllListeners();
        txtName.onEndEdit.AddListener(SaveName);
        txtAge.onEndEdit.RemoveAllListeners();
        txtAge.onEndEdit.AddListener(SaveAge);
        GameUtil.ButtonOnClick(btnLogout, OnClickLogout);

        if (txtSubject)
        {
            txtSubject.text = DataManager.Instance.Data.subjectName;
        }
        if (txtName)
        {
            txtName.text = DataManager.Instance.Data.namePlayer;
        }
        if (txtAge)
        {
            txtAge.text = DataManager.Instance.Data.age.ToString();
        }
        if (bsPersona)
        {
            bsPersona.Select((int)PersonaManager.Instance.GetMyPersonaType());
        }
    }

    private void OnClickLogout()
    {
        if (btnLogout) btnLogout.interactable = false;

        AuthManager.Instance.Logout(() =>
        {
            if (btnLogout) btnLogout.interactable = true;
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.Login);
        });
    }

    private void SaveSubject(string subject)
    {
        DataManager.Instance.Data.subjectName = subject;
        DataManager.Instance.SaveGameData();
    }
    private void SaveName(string name)
    {
        DataManager.Instance.Data.namePlayer = name;
        DataManager.Instance.SaveGameData();
    }
    private void SaveAge(string age)
    {
        try
        {
            DataManager.Instance.Data.age = Int32.Parse(age);
        }
        catch (FormatException)
        {
            Debug.LogError("Invalid age format");
        }
        DataManager.Instance.SaveGameData();
    }
}

using Luzart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
public class Tutorial_Scene3 : MonoBehaviour
{
    public TMP_InputField inputField;
    public StepActionPublicAction stepAction;
    public string strPrompt = "How can I make fractions fun for students?";
    private string _strInput = "";

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnChangeInputField);
    }
    public void OnChangeInputField(string value)
    {
        _strInput = value;
    }
    public void OnClickSend()
    {
        if (!GameUtil.MatchByWordRatio(_strInput, strPrompt, 0.5f))
        {
            var ui = UIManager.Instance.ShowUI<UINoti>(UIName.Noti);
            ui.InitPopup(Loc.T("Please enter the correct prompt!"));
        }
        else
        {
            stepAction.OnClickOnDone();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldSend : MonoBehaviour
{
    public Button btnSend;
    public TMP_InputField inputField;
    public UnityEvent<string> OnClickHandle;
    [SerializeField,ReadOnly] 
    private string _currentStr;
    private void Reset()
    {
        btnSend ??= GetComponentInChildren<Button>();
        inputField ??= GetComponentInChildren<TMP_InputField>();
    }
    private void Awake()
    {
        inputField.onValueChanged.RemoveAllListeners();
        inputField.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(string.Empty);
    }
    private void OnValueChanged(string value)
    {
        btnSend.interactable = !string.IsNullOrWhiteSpace(value);
        _currentStr = value;
    }
    public void OnClick()
    {
        OnClickHandle?.Invoke(_currentStr);
        inputField.text = "";
    }
    public void ImportToThis(string str)
    {
        _currentStr = str;
        inputField.text = str;
    }
}

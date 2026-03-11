using UnityEngine;
using UnityEditor;
using TMPro;

public class TMP_InputFieldEditorExtension
{
    // "CONTEXT" giúp menu xuất hiện khi chuột phải vào Component
    // "TMP_InputField" là loại Component áp dụng
    // "Setup Input" là tên hiển thị trong menu
    [MenuItem("CONTEXT/TMP_InputField/Setup Custom Input")]
    public static void SetupInput(MenuCommand command)
    {
        // Lấy component mà bạn vừa chuột phải vào
        TMP_InputField inputField = (TMP_InputField)command.context;

        // Thực hiện các thiết lập tự động
        Undo.RecordObject(inputField, "Setup Input Field"); // Cho phép Ctrl+Z

        var textComponent = inputField.gameObject.GetComponent<TMP_Text>();
        textComponent.raycastTarget = true;
        string str = textComponent.text;
        inputField.textComponent = textComponent;
        inputField.pointSize = textComponent.fontSize;
        inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
        inputField.fontAsset = textComponent.font;
        inputField.readOnly = true;
        inputField.transition = UnityEngine.UI.Selectable.Transition.None;
        inputField.text = str;
    }

}
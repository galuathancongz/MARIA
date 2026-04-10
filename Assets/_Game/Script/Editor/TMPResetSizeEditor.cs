#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

public static class TMPResetSizeEditor
{
    // Padding cong them moi ben (trai/phai/tren/duoi)
    private const float Padding = 5f;
    // fontSizeMin = currentFontSize * MinFontSizeRatio
    private const float MinFontSizeRatio = 0.5f;

    [MenuItem("CONTEXT/TextMeshProUGUI/Reset Size")]
    public static void ResetSizeUGUI(MenuCommand command)
    {
        ResetSize((TMP_Text)command.context);
    }

    [MenuItem("CONTEXT/TextMeshPro/Reset Size")]
    public static void ResetSize3D(MenuCommand command)
    {
        ResetSize((TMP_Text)command.context);
    }

    private static void ResetSize(TMP_Text tmp)
    {
        Undo.RecordObject(tmp, "TMP Reset Size");
        Undo.RecordObject(tmp.rectTransform, "TMP Reset Size");

        // Luu font size hien tai truoc khi thay doi
        float currentFontSize = tmp.fontSize;

        // Tam tat wrapping va auto-size de lay preferred size chinh xac (single-line)
        bool wasAutoSize = tmp.enableAutoSizing;
        bool wasWrapping = tmp.enableWordWrapping;
        float wasFontSize = tmp.fontSize;

        tmp.enableAutoSizing = false;
        tmp.enableWordWrapping = false;
        tmp.fontSize = currentFontSize;

        // GetPreferredValues() tra ve kich thuoc text khong bi rang buoc boi RectTransform
        Vector2 preferredSize = tmp.GetPreferredValues();

        // Set RectTransform = preferred size + padding moi ben
        tmp.rectTransform.sizeDelta = new Vector2(
            preferredSize.x + Padding * 2f,
            preferredSize.y + Padding * 2f
        );

        // Bat wrapping de text wrap theo boundary moi
        tmp.enableWordWrapping = true;

        // Bat Auto Size voi max = font size hien tai
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = currentFontSize;
        tmp.fontSizeMin = Mathf.Max(1f, currentFontSize * MinFontSizeRatio);

        tmp.ForceMeshUpdate();

        EditorUtility.SetDirty(tmp);
        EditorUtility.SetDirty(tmp.rectTransform);

        Debug.Log($"[TMP Reset Size] {tmp.gameObject.name}: " +
                  $"Size=({tmp.rectTransform.sizeDelta.x:F1}, {tmp.rectTransform.sizeDelta.y:F1}), " +
                  $"AutoSize={tmp.fontSizeMin:F1}-{tmp.fontSizeMax:F1}");
    }
}
#endif

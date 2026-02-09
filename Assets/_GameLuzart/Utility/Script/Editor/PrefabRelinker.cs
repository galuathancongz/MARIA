using UnityEditor;
using UnityEngine;

public class PrefabRelinker : EditorWindow
{
    private GameObject sourcePrefab; // Prefab gốc (Cha)
    private GameObject duplicatedObject; // Bản lỡ tay duplicate

    [MenuItem("Luzart/LuzartTool/Convert Duplicate to Variant")]
    public static void ShowWindow()
    {
        GetWindow<PrefabRelinker>("Relinker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Biến bản Duplicate thành Variant", EditorStyles.boldLabel);
        sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab Gốc (Parent)", sourcePrefab, typeof(GameObject), false);
        duplicatedObject = (GameObject)EditorGUILayout.ObjectField("Bản Duplicate", duplicatedObject, typeof(GameObject), false);

        if (GUILayout.Button("Kết nối lại thành Variant"))
        {
            if (sourcePrefab != null && duplicatedObject != null)
            {
                PrefabUtility.ReplacePrefabAssetOfPrefabInstance(duplicatedObject, sourcePrefab,InteractionMode.AutomatedAction);
            }
        }
    }
}
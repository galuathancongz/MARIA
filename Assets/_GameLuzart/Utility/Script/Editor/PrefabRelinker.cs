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
                string path = AssetDatabase.GetAssetPath(duplicatedObject);
                // Tạo Variant mới dựa trên Prefab gốc nhưng tại vị trí của bản duplicate
                GameObject variant = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);

                // Ghi đè cấu trúc của bản duplicate lên Variant mới này
                EditorUtility.CopySerializedIfDifferent(duplicatedObject, variant);

                // Lưu thành file Variant đè lên file cũ
                PrefabUtility.SaveAsPrefabAssetAndConnect(variant, path, InteractionMode.AutomatedAction);

                DestroyImmediate(variant);
                AssetDatabase.SaveAssets();
                Debug.Log("<color=green>Thành công!</color> Bản duplicate đã trở thành Variant của " + sourcePrefab.name);
            }
        }
    }
}
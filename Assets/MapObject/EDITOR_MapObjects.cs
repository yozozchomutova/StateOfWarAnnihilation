#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

[CustomEditor(typeof(MapObject)), CanEditMultipleObjects]
public class EDITOR_MapObjects : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector layout

        if (GUILayout.Button("Regenerate thumbnails - THIS"))
        {
            Debug.Log("Building thumbnail...");

            ProcessObject((MapObject)target);

            Debug.Log("Finished");
        } else if (GUILayout.Button("Regenerate thumbnails - ALL"))
        {
            Debug.Log("Building all thumbnails...");

            MapObject[] objects = Resources.LoadAll<MapObject>("Map Objects");
            foreach (MapObject obj in objects)
                ProcessObject(obj);

            Debug.Log("Finished");
        }
    }

    private void ProcessObject(MapObject obj)
    {
        Debug.Log("Building MAP OBJECT: " + obj.name);
        string path = "/MapObject/GeneratedIcons/" + obj.name + ".png";
        EditorUtility.SetDirty(obj);
        Texture2D preview = AssetPreview.GetAssetPreview(obj.gameObject);
        byte[] png = preview.EncodeToPNG();

        File.WriteAllBytes(Application.dataPath + path, png);
        AssetDatabase.ImportAsset("Assets/" + path);
        AssetDatabase.Refresh();
        Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/" + path);

        obj.icon = t;
    }
}
#endif
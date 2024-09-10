#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitReference), true), CanEditMultipleObjects]
public class EDITOR_Unit : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector layout

        if (GUILayout.Button("Build Unit"))
        {
            Debug.Log("Building unit...");
            EditorUtility.SetDirty(target);
            UnitReference unit = (UnitReference)target;
            unit.EditorsInit();
            Debug.Log("Finished");
        } else if (GUILayout.Button("Build ALL Units"))
        {
            Debug.Log("Building All Units & Bodies...");
            UnitReference[] unitPrefabs = Resources.LoadAll<UnitReference>("UnitsNew");
            UnitReference[] bodyPrefabs = Resources.LoadAll<UnitReference>("Bodies");

            foreach (UnitReference unitPref in unitPrefabs)
            {
                Debug.Log("Building UNIT: " + unitPref.name);
                EditorUtility.SetDirty(unitPref);
                unitPref.EditorsInit();
            }

            foreach (UnitReference bodyPref in bodyPrefabs)
            {
                Debug.Log("Building BODY: " + bodyPref.name);
                EditorUtility.SetDirty(bodyPref);
                bodyPref.EditorsInit();
            }

            Debug.Log("Finished");
        }
    }
}
#endif
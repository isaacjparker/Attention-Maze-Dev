using UnityEngine;
using UnityEditor;

// Tell Unity this custom editor is for POIGroup
[CustomEditor(typeof(POIGroup))]
public class POIGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the default fields (POIPrefabs, InstantiateRandom, poiList)
        DrawDefaultInspector();

        POIGroup myTarget = (POIGroup)target;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Instantiate Prefabs"))
        {
            // When the user clicks this button, call our method
            myTarget.InstantiatePrefabs();

            // Mark dirty so the scene knows something changed
            EditorUtility.SetDirty(myTarget);
        }
        GUI.backgroundColor = Color.white;
    }
}

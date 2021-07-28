using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor {
    

    public override void OnInspectorGUI() {
        MapGenerator mapGenerator = (MapGenerator) target;

        // If data was modified
        if (DrawDefaultInspector()) {
            if (mapGenerator.autoUpdate) {
                mapGenerator.DrawMap();
            }
        }
        
        GUI.enabled = ((MapGenerator) target).drawMode != MapGenerator.DrawMode.Chunks;
        if (GUILayout.Button("Generate")) {
            mapGenerator.DrawMap();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Clear")) {
            mapGenerator.ClearMap();
        }
    }
}

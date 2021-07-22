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

        if (GUILayout.Button("Generate")) {
            mapGenerator.DrawMap();
        }
    }
}

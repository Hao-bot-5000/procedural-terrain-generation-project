using UnityEngine;

public class TerrainMovement : MonoBehaviour {
    public float scale = 0.275f; // Not sure what this does
    public float strength = 0.5f;
    public float frequency = 0.25f;

    bool isVisible = false;
    float xOffset = 0f;
    float yOffset = 0f;
    MeshFilter terrainFilter;

    void Start() {
        terrainFilter = GetComponent<MeshFilter>();
    }

    void Update() {
        if (isVisible && (terrainFilter != null || terrainFilter.sharedMesh != null)) UpdateTerrainObject();
        xOffset += frequency * Time.deltaTime;
        yOffset += frequency * Time.deltaTime;
    }

    void OnBecameVisible() {
        isVisible = true;
    }

    void OnBecameInvisible() {
        isVisible = false;
    }

    // TODO: Only adds up/down terrain movement (water waves) - try expanding this to be useful for
    //       other terrain objects (i.e. trees, grass).
    private void UpdateTerrainObject() {
        Vector3[] vertices = terrainFilter.sharedMesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            // Vector3 vertexPosition = transform.TransformPoint(vertices[i]) / 10; // FIXME: hardcoded mesh scale value -- apparently getting transform.localScale is somewhat expensive?
            // vertices[i].y = UpdateWaveHeight(vertexPosition.x, vertexPosition.z);
            UpdateWaveHeightAtPosition(ref vertices[i]);
        }

        terrainFilter.sharedMesh.vertices = vertices;
        terrainFilter.sharedMesh.RecalculateNormals();
    }

    private void UpdateWaveHeightAtPosition(ref Vector3 vertex) {
        Vector3 vertexPosition = transform.TransformPoint(vertex) / 10;

        float scaleX = vertexPosition.x * scale + xOffset;
        float scaleZ = vertexPosition.z * scale + yOffset;

        vertex.y = (Mathf.PerlinNoise(scaleX, scaleZ) * 2 - 1) * strength;
    }

    public float GetWaveHeightAtPosition(Vector3 vertex) {
        float scaleX = vertex.x * scale + xOffset;
        float scaleZ = vertex.z * scale + yOffset;

        return vertex.y + (Mathf.PerlinNoise(scaleX, scaleZ) * 2 - 1) * strength;
    }
}
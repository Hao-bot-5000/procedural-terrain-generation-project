using UnityEngine;

public class TerrainMovement : MonoBehaviour {
    public float scale; // What does this even do...
    public float strength;
    public float frequency;

    float xOffset = 0f;
    float yOffset = 0f;
    MeshFilter terrainFilter;

    void Start() {
        terrainFilter = GetComponent<MeshFilter>();
    }

    void Update() {
        if (terrainFilter != null || terrainFilter.sharedMesh != null) UpdateTerrain();
        xOffset += frequency * Time.deltaTime;
        yOffset += frequency * Time.deltaTime;
    }

    // TODO: Only adds up/down terrain movement (water waves) - try expanding this to be useful for
    //       other terrain objects (i.e. trees, grass).
    private void UpdateTerrain() {
        Vector3[] vertices = terrainFilter.sharedMesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            vertices[i].y = UpdateTerrainVertex(vertices[i].x, vertices[i].z);
        }

        terrainFilter.sharedMesh.vertices = vertices;
        terrainFilter.sharedMesh.RecalculateNormals();
    }

    private float UpdateTerrainVertex(float x, float y) {
        float scaleX = x * scale + xOffset;
        float scaleY = y * scale + yOffset;

        return Mathf.PerlinNoise(scaleX, scaleY) * strength;
    }
}
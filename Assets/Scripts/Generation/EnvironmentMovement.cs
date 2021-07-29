using UnityEngine;

public class EnvironmentMovement : MonoBehaviour {
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
        if (isVisible && (terrainFilter != null || terrainFilter.sharedMesh != null)) UpdateEnvironmentObject();
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
    private void UpdateEnvironmentObject() {
        Vector3[] vertices = terrainFilter.sharedMesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 vertexPosition = transform.TransformPoint(vertices[i]) / 10; // FIXME: hardcoded mesh scale value -- apparently getting transform.localScale is somewhat expensive?
            vertices[i].y = UpdateVertex(vertexPosition.x, vertexPosition.z);
        }

        terrainFilter.sharedMesh.vertices = vertices;
        terrainFilter.sharedMesh.RecalculateNormals();
    }

    private float UpdateVertex(float x, float y) {
        float scaleX = x * scale + xOffset;
        float scaleY = y * scale + yOffset;

        return Mathf.PerlinNoise(scaleX, scaleY) * strength;
    }
}
using UnityEngine;

public static class MeshGenerator {

    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve inputHeightCurve, int levelOfDetail) {
        AnimationCurve heightCurve = new AnimationCurve(inputHeightCurve.keys);
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = levelOfDetail != 0 ? levelOfDetail * 2 : 1;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;
        for (int z = 0; z < height; z += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                // Flooring vertexHeight to generate cube-like generation 
                float vertexHeight = Mathf.Round(heightMap[x, z] * (heightMultiplier));
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ - z);
                meshData.uvs[vertexIndex] = new Vector2(x / (float) width, z / (float) height);

                // Create triangles
                if (x < width - 1 && z < height - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                
                vertexIndex++;
            }
        }

        meshData.UseFlatShading();

        return meshData;
    }

    // Use this if we want "smoother" cube-like generation
    private static float MapValueOntoSmoothStaircaseFunction(float x, int numCalculations) {
        // https://newbedev.com/equation-for-a-smooth-staircase-function
        float tau = 2 * Mathf.PI;
        float result = x - (Mathf.Sin(tau * x) / tau);
        return numCalculations > 1 ? MapValueOntoSmoothStaircaseFunction(result, numCalculations - 1) : result;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
    }

    public void AddTriangle(int v1, int v2, int v3) {
        triangles[triangleIndex] = v1;
        triangles[triangleIndex + 1] = v2;
        triangles[triangleIndex + 2] = v3;
        triangleIndex += 3;
    }

    public void UseFlatShading() {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUVs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++) {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUVs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUVs;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;

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

    // public void MakeDoubleSided() {
    //     List<int> doubleSidedTriangles = new List<int>(triangles);

    //     int[] reversedTriangles = new int[triangles.Length];
    //     for (int i = 0; i < triangles.Length; i++) {
    //         reversedTriangles[i] = triangles[triangles.Length - 1 - i];
    //     }

    //     doubleSidedTriangles.AddRange(reversedTriangles);

    //     triangles = doubleSidedTriangles.ToArray();
    // }

    public Mesh CreateMesh(bool isDynamic=false) {
        Mesh mesh = new Mesh();

        // Mesh max vertex count from ~65k (16-bit) -> ~4b (32-bit)
        if (vertices.Length > ushort.MaxValue) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        if (isDynamic) mesh.MarkDynamic();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.Optimize();
        return mesh;
    }
}

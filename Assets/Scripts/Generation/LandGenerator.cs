using UnityEngine;
using System;
using System.Collections.Generic;

public static class LandGenerator {
    public static MeshData GenerateLandMesh(float[,] heightMap, float heightMultiplier, int levelOfDetail) {
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
                float vertexHeight = heightMap[x, z] * heightMultiplier;
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ - z);
                meshData.uvs[vertexIndex] = new Vector2(x / (float) width, z / (float) height);

                // Create triangles
                if (x < width - 1 && z < height - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine, vertexIndex + 1, vertexIndex + verticesPerLine + 1);
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

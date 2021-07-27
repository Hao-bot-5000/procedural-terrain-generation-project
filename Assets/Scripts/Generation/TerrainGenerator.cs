using UnityEngine;
using System;
using System.Collections.Generic;

public static class TerrainGenerator {
    public static List<MeshData> GenerateTerrainMeshes(float[,] heightMap, float heightMultiplier, int chunkSize, int mapSize, AnimationCurve inputHeightCurve) {
        chunkSize++;
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        List<MeshData> chunkList = new List<MeshData>();

        // Really bad runtime, but best simple solution I can come up with (that does not require me to rewrite everything)
        for (int i = 0; i < mapSize * mapSize; i++) {
            float[,] chunk = new float[chunkSize, chunkSize];
            int offsetX = (i % mapSize) * (chunkSize - (i % mapSize != 0 ? 1 : 0));
            int offsetZ = (i / mapSize) * (chunkSize - (i >= mapSize ? 1 : 0));
            // Debug.Log(i + ": " + offsetX + " " + offsetZ);
            for (int z = 0; z < chunkSize; z++) {
                for (int x = 0; x < chunkSize; x++) {
                    // Debug.Log(i + " - [" + x + ", " + z + "] - " + (x + offsetX) + " " + (z + offsetZ));
                    chunk[x, z] = heightMap[x + offsetX, z + offsetZ];
                }
            }
            chunkList.Add(GenerateTerrainMesh(chunk, heightMultiplier, inputHeightCurve, 1)); // FIXME: hardcoded LOD value of 1
        }

        return chunkList;
    }

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
                float vertexHeight = Mathf.Round(heightCurve.Evaluate(heightMap[x, z]) * (heightMultiplier));
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

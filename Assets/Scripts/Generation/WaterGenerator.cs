using UnityEngine;
using System.Collections.Generic;

public static class WaterGenerator {
    // APPARENTLY DECENT DOCUMENTATION ON SHADERS AND TUTORIALS ON 
    // HOW TO GENERATE FLATSHADED LOWPOLY WATER SHADERS ARE NONEXISTENT
    // public static List<MeshData> GenerateWaterMeshes(int chunkSize, int mapSize) {
    //     chunkSize++;

    //     List<MeshData> chunkList = new List<MeshData>();

    //     for (int i = 0; i < mapSize * mapSize; i++) {
    //         chunkList.Add(GenerateWaterMesh(chunkSize, chunkSize, 1)); // FIXME: hardcoded LOD value of 1
    //     }

    //     return chunkList;
    // }

    public static MeshData GenerateWaterMesh(int width, int height, int levelOfDetail) {
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = levelOfDetail != 0 ? levelOfDetail * 2 : 1;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;
        for (int z = 0; z < height; z += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                float vertexHeight = 0;
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

        // meshData.MakeDoubleSided();
        meshData.UseFlatShading();

        return meshData;
    }
}
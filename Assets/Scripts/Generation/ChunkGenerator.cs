using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkGenerator {
    
    public static List<ChunkData> GenerateChunks(float[,] heightMap, Color[] colorMap, int chunkSize, int mapSize, int verticesPerSide, float heightMultiplier, AnimationCurve inputHeightCurve) {
        chunkSize++;
        List<ChunkData> chunkList = new List<ChunkData>();

        for (int i = 0; i < mapSize * mapSize; i++) {
            float[,] chunkHeightMap = new float[chunkSize, chunkSize];
            Color[] chunkColorMap = new Color[chunkSize * chunkSize];

            int offsetX = (i % mapSize) * (chunkSize - (i % mapSize != 0 ? 1 : 0));
            int offsetZ = (i / mapSize) * (chunkSize - (i >= mapSize ? 1 : 0));

            for (int z = 0; z < chunkSize; z++) {
                for (int x = 0; x < chunkSize; x++) {
                    chunkHeightMap[x, z] = heightMap[x + offsetX, z + offsetZ];
                    chunkColorMap[z * chunkSize + x] = colorMap[(z + offsetZ) * verticesPerSide + (x + offsetX)];
                }
            }

            ChunkData chunkData = new ChunkData(chunkSize, 1, chunkHeightMap, heightMultiplier, inputHeightCurve);
            chunkData.UpdateLandTexture(TextureGenerator.TextureFromColorMap(chunkColorMap, chunkSize, chunkSize));
            chunkList.Add(chunkData);
        }
    
        return chunkList;
    }
}

public class ChunkData {
    public MeshData landMeshData;
    public Texture2D landTexture;
    public MeshData waterMeshData;
    // Texture2D waterTexture;
    public List<GameObject> items;

    public int size;
    public int lod; // useful for future performance optimizations

    public ChunkData(int size, int lod, float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve) {
        this.size = size;
        this.lod = lod;

        landMeshData = LandGenerator.GenerateLandMesh(heightMap, heightMultiplier, heightCurve, lod);
        waterMeshData = WaterGenerator.GenerateWaterMesh(size, size, lod);
        items = new List<GameObject>();
    }

    public void UpdateLandTexture(Texture2D texture) {
        landTexture = texture;
    }

    // public void UpdateWaterTexture(Texture2D texture) {
    //     waterTexture = texture;
    // }

    public void AddObject(GameObject item) {
        items.Add(item);
    }

    public void AddObjects(List<GameObject> items) {
        items.AddRange(items);
    }
}

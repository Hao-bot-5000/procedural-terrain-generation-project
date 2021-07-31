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

            MeshData landMeshData = LandGenerator.GenerateLandMesh(chunkHeightMap, heightMultiplier, inputHeightCurve, 1); // FIXME: hardcoded LOD value of 1
            Texture2D landTexture = TextureGenerator.TextureFromColorMap(chunkColorMap, chunkSize, chunkSize);
            MeshData waterMeshData = WaterGenerator.GenerateWaterMesh(chunkSize, chunkSize, 1); // FIXME: hardcoded LOD value of 1

            chunkList.Add(new ChunkData(landMeshData, landTexture, waterMeshData));
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

    public ChunkData(MeshData landMeshData, Texture2D landTexture, MeshData waterMeshData) {
        this.landMeshData = landMeshData;
        this.landTexture = landTexture;
        this.waterMeshData = waterMeshData;
        this.items = new List<GameObject>();
    }

    public void AddObject(GameObject item) {
        items.Add(item);
    }

    public void AddObjects(List<GameObject> items) {
        items.AddRange(items);
    }
}

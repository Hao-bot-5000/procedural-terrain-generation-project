using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkGenerator {
    public static List<ChunkData> GenerateChunks(MapData mapData, int chunkSize, int mapSize, int verticesPerSide, float scale, float heightMultiplier, int lod, Dictionary<ThingType, List<GameObject>> thingPrefabs) {
        System.Random seedRNG = mapData.seedRNG;
        float[,] heightMap = mapData.heightMap;
        Color[] colorMap = mapData.colorMap;
        float[,] treeMap = mapData.treeMap;

        List<GameObject> treePrefabs = thingPrefabs.ContainsKey(ThingType.Tree) ? thingPrefabs[ThingType.Tree] : null;
        
        int numChunks = mapSize * mapSize;
        int chunkVertices = chunkSize + 1;
        float topLeftX = -(chunkSize * 0.5f) * (mapSize - 1);
        float topLeftZ =  (chunkSize * 0.5f) * (mapSize - 1);

        List<ChunkData> chunkList = new List<ChunkData>();

        for (int i = 0; i < numChunks; i++) {
            float[,] chunkHeightMap = new float[chunkVertices, chunkVertices];
            Color[] chunkColorMap = new Color[chunkVertices * chunkVertices];
            List<ThingData> chunkThings = new List<ThingData>();

            Vector3 chunkPosition = new Vector3(topLeftX + (i % mapSize) * chunkSize, 0, topLeftZ - ((i / mapSize)) * chunkSize);
            int offsetX = (i % mapSize) * (chunkVertices - (i % mapSize != 0 ? 1 : 0));
            int offsetZ = (i / mapSize) * (chunkVertices - (i >= mapSize ? 1 : 0));

            for (int z = 0; z < chunkVertices; z++) {
                for (int x = 0; x < chunkVertices; x++) {
                    chunkHeightMap[x, z] = heightMap[x + offsetX, z + offsetZ];
                    chunkColorMap[z * chunkVertices + x] = colorMap[(z + offsetZ) * verticesPerSide + (x + offsetX)];
                
                    // Do not spawn trees on chunk edges
                    if ((z > 0 && z < chunkVertices) && (x > 0 && x < chunkVertices)) {
                        if (seedRNG.NextDouble() < treeMap[x + offsetX, z + offsetZ]) {
                            Vector3 treePosition = new Vector3(
                                chunkPosition.x - (chunkSize * 0.5f) + x, 
                                chunkHeightMap[x, z] * heightMultiplier, 
                                chunkPosition.z + (chunkSize * 0.5f) - z
                            );
                            TreeGenerator.AddTreeData(ref chunkThings, treePosition, treePrefabs[seedRNG.Next(0, treePrefabs.Count)]);
                        }
                    }
                }
            }

            ChunkData chunkData = new ChunkData(chunkPosition, chunkSize, scale, lod, chunkHeightMap, heightMultiplier);
            chunkData.SetLandTexture(TextureGenerator.TextureFromColorMap(chunkColorMap, chunkVertices, chunkVertices));
            chunkData.AddThings(chunkThings);
            chunkList.Add(chunkData);
        }
    
        return chunkList;
    }
}

public class ChunkData {
    public MeshData landMeshData { get; }
    public Texture2D landTexture { get; private set; }
    public MeshData waterMeshData { get; }
    // Texture2D waterTexture;
    public List<ThingData> things { get; private set; }

    public Vector3 position { get; }
    public int size { get; }
    public float scale { get; }
    public int lod { get; private set; } // useful for future performance optimizations

    public ChunkData(Vector3 position, int size, float scale, int lod, float[,] heightMap, float heightMultiplier) {
        this.position = position;
        this.size = size;
        this.scale = scale;
        this.lod = lod;

        landMeshData = LandGenerator.GenerateLandMesh(heightMap, heightMultiplier, lod);
        waterMeshData = WaterGenerator.GenerateWaterMesh(size + 1, size + 1, lod);
        things = new List<ThingData>();
    }

    public void SetLandTexture(Texture2D texture) {
        landTexture = texture;
    }

    // public void UpdateWaterTexture(Texture2D texture) {
    //     waterTexture = texture;
    // }

    public void AddThing(ThingData thing) {
        things.Add(thing);
    }

    public void AddThings(List<ThingData> things) {
        this.things.AddRange(things);
    }
}

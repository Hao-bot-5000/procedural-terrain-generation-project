using UnityEngine;
using System;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
    // NOTE: having many chunks with their own unique materials causes SetPass calls to skyrocket
    public const int chunkSize = 24;
    public const int mapSize = 30; // n x n chunks
    const int verticesPerSide = chunkSize * mapSize + 1;

    public const float waterLevel = 11.5f;
    public const float meshScale = 10f;

	public enum DrawMode { NoiseMap, ColorMap, FalloffMap, TreeMap, Mesh, Chunks };
    public DrawMode drawMode;

    [Range (0, 4)]
    public int previewLOD;

    public float noiseScale;

    public int seed;
    public Vector2 offset;

    public bool useFalloff;

    public int octaves;
    [Range (0, 1)]
    public float persistance;
    public float lacunarity;

    public float meshHeightMultiplier;
    public AnimationCurve heightCurve;
    public bool displayWaterMesh;
    public bool displayTrees;

    public List<GameObject> treePrefabs;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

	public MapData GenerateMapData(Vector2 center) {
        MapData mapData = new MapData(seed);

		float[,] noiseMap = Noise.GenerateNoiseMap(verticesPerSide, verticesPerSide, mapData.seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset, useFalloff ? falloffMap : null, heightCurve, 1 / meshHeightMultiplier);
        Color[] colorMap = new Color[verticesPerSide * verticesPerSide];

        for (int z = 0; z < verticesPerSide; z++) {
            for (int x = 0; x < verticesPerSide; x++) {

                float currentHeight = noiseMap[x, z];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height) {
                        colorMap[z * verticesPerSide + x] = regions[i].color;
                    }
                    else {
                        break;
                    }
                }
            }
        }

        mapData.SetHeightMap(noiseMap);
        mapData.SetColorMap(colorMap);

        if (displayTrees) {
            float[,] treeMap = TreeGenerator.GenerateTreeMap(mapData.heightMap, mapData.seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset, waterLevel / meshHeightMultiplier, 0.25f);
            mapData.SetTreeMap(treeMap);
        }

        return mapData;
	}

    public void DrawMap() {
        Vector2 center = Vector2.zero;

        MapData mapData = GenerateMapData(center);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        Dictionary<ThingType, List<GameObject>> thingPrefabs = new Dictionary<ThingType, List<GameObject>>();
        thingPrefabs.Add(ThingType.Tree, treePrefabs);

        switch (drawMode) {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, verticesPerSide, verticesPerSide));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(verticesPerSide)));
                break;
            case DrawMode.TreeMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(TreeGenerator.GenerateTreeMap(mapData.heightMap, mapData.seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset, waterLevel / meshHeightMultiplier, 0.25f)));
                break;
            case DrawMode.Mesh:
                MeshData landMesh = LandGenerator.GenerateLandMesh(mapData.heightMap, meshHeightMultiplier, previewLOD);
                display.DrawLandMesh(landMesh, TextureGenerator.TextureFromColorMap(mapData.colorMap, verticesPerSide, verticesPerSide));
                // display.DrawTrees(landMesh, displayTrees ? TreeGenerator.GenerateTreeMap(mapData.heightMap, seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset, 0.25f) : null);
                display.DrawWaterMesh(displayWaterMesh ? WaterGenerator.GenerateWaterMesh(verticesPerSide, verticesPerSide, previewLOD) : null);
                break;
            case DrawMode.Chunks:
                List<ChunkData> chunkDataList = ChunkGenerator.GenerateChunks(mapData, chunkSize, mapSize, verticesPerSide, meshScale, meshHeightMultiplier, previewLOD, thingPrefabs);
                display.DrawChunks(chunkDataList, /*chunkSize, */mapSize, meshHeightMultiplier, waterLevel);
                break;
            default: break;
        }
    }

    public void ClearMap() {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.ClearDisplay();
    }
	
    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(verticesPerSide);
        DrawMap();
    }

    void OnValidate() {
        if (falloffMap == null) falloffMap = FalloffGenerator.GenerateFalloffMap(verticesPerSide);
    }
}

[Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    // https://stackoverflow.com/questions/48795701/how-to-change-readonly-variables-value/48795846
    public int seed { get; }
    public System.Random seedRNG { get; }

    public float[,] heightMap { get; private set; }
    public Color[] colorMap { get; private set; }
    public float[,] treeMap { get; private set; }

    public MapData(int seed) {
        this.seed = seed;
        seedRNG = new System.Random(seed);

        this.heightMap = null;
        this.colorMap = null;
        this.treeMap = null;
    }

    public void SetHeightMap(float[,] heightMap) {
        this.heightMap = heightMap;
    }

    public void SetColorMap(Color[] colorMap) {
        this.colorMap = colorMap;
    }

    public void SetTreeMap(float[,] treeMap) {
        this.treeMap = treeMap;
    }
}

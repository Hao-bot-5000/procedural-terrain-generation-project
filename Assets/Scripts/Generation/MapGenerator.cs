using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode { NoiseMap, ColorMap, FalloffMap, Mesh };
    public DrawMode drawMode;

    public const int mapSize = 97;

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
    public AnimationCurve meshHeightCurve;
    public bool displayWaterMesh;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

    System.Random seedRNG;

	public MapData GenerateMapData(Vector2 center) {
		float[,] noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset);

        Color[] colorMap = new Color[mapSize * mapSize];
        for (int z = 0; z < mapSize; z++) {
            for (int x = 0; x < mapSize; x++) {
                if (useFalloff) {
                    noiseMap[x, z] = Mathf.Clamp(noiseMap[x, z] - falloffMap[x, z], 0, 1);
                }

                float currentHeight = noiseMap[x, z];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height) {
                        colorMap[z * mapSize + x] = regions[i].color;
                    }
                    else {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
	}

    public void DrawMap() {
        seedRNG = new System.Random(seed);

        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch (drawMode) {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapSize, mapSize));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapSize)));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(TerrainGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, previewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapSize, mapSize));
                display.DrawWater(displayWaterMesh ? WaterGenerator.GenerateWaterMesh(mapSize, mapSize, previewLOD) : null); // NOTE: water
                break;
            default: break;
        }
    }
	
    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize);
        DrawMap();
    }

    void OnValidate() {
        if (falloffMap == null) falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize);
    }
}

[Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}

using UnityEngine;
using System;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
    public const int chunkSize = 24;
    public const int mapSize = 30; // 30 x 30 chunks
    const int verticesPerSide = chunkSize * mapSize + 1;

    public const float waterLevel = 11f;
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
    public AnimationCurve meshHeightCurve;
    public bool displayWaterMesh;
    public bool displayTrees;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

    System.Random seedRNG;

	public MapData GenerateMapData(Vector2 center) {
		float[,] noiseMap = Noise.GenerateNoiseMap(verticesPerSide, verticesPerSide, seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset);

        Color[] colorMap = new Color[verticesPerSide * verticesPerSide];
        for (int z = 0; z < verticesPerSide; z++) {
            for (int x = 0; x < verticesPerSide; x++) {
                if (useFalloff) {
                    noiseMap[x, z] = Mathf.Clamp(noiseMap[x, z] - falloffMap[x, z], 0, 1);
                }

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

        return new MapData(noiseMap, colorMap);
	}

    public void DrawMap() {
        Vector2 center = Vector2.zero;
        seedRNG = new System.Random(seed);

        MapData mapData = GenerateMapData(center);
        MapDisplay display = FindObjectOfType<MapDisplay>();

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
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(TreeGenerator.GenerateTreeMap(mapData.heightMap, seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset, 0.25f)));
                break;
            case DrawMode.Mesh:
                MeshData landMesh = LandGenerator.GenerateLandMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, previewLOD);
                display.DrawLandMesh(landMesh, TextureGenerator.TextureFromColorMap(mapData.colorMap, verticesPerSide, verticesPerSide));
                // display.DrawTrees(landMesh, displayTrees ? TreeGenerator.GenerateTreeMap(mapData.heightMap, seedRNG, noiseScale, octaves, persistance, lacunarity, center + offset, 0.25f) : null);
                display.DrawWaterMesh(displayWaterMesh ? WaterGenerator.GenerateWaterMesh(verticesPerSide, verticesPerSide, previewLOD) : null);
                break;
            case DrawMode.Chunks:
                List<MeshData> landMeshes = LandGenerator.GenerateLandMeshes(mapData.heightMap, meshHeightMultiplier, chunkSize, mapSize, meshHeightCurve);
                List<Texture2D> landTextures = TextureGenerator.TexturesFromColorMap(mapData.colorMap, chunkSize, mapSize, verticesPerSide);
                List<MeshData> waterMeshes = WaterGenerator.GenerateWaterMeshes(chunkSize, mapSize);
                display.DrawLandMeshes(landMeshes, landTextures, chunkSize, mapSize, meshScale);
                display.DrawWaterMeshes(waterMeshes, chunkSize, mapSize, waterLevel, meshScale);
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
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}

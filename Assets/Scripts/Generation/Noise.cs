using UnityEngine;

public static class Noise {

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, System.Random seedRNG, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        Vector2[] octaveOffsets = new Vector2[octaves];

        float amplitude = 1f;
        float frequency = 1f;

        float maxPossibleHeight = 0;

        for (int i = 0; i < octaves; i++) {
            float offsetX = seedRNG.Next(-100000, 100000) + offset[0];
            float offsetY = seedRNG.Next(-100000, 100000) - offset[1];
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Generate perlin value for each map/table index
        for (int z = 0; z < mapWidth; z++) {
            for (int x = 0; x < mapHeight; x++) {
                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i][0]) / scale * frequency;
                    float sampleY = (z - halfHeight + octaveOffsets[i][1]) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
                else if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;

                noiseMap[x, z] = noiseHeight;
            }
        }

        for (int z = 0; z < mapWidth; z++) {
            for (int x = 0; x < mapHeight; x++) {
                float noise = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, z]);
                noiseMap[x, z] = noise;
            }
        }

        return noiseMap;
    }
}
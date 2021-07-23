using UnityEngine;
using System;

public static class TreeGenerator {

    public static float[,] GenerateTreeMap(float[,] terrainMap, System.Random seedRNG, float scale, int octaves, float persistance, float lacunarity, Vector2 centerPlusOffset, float discreteness) {
        int width = terrainMap.GetLength(0);
        int height = terrainMap.GetLength(1);

        float[,] map = Noise.GenerateNoiseMap(width, height, seedRNG, scale, octaves, persistance, lacunarity, centerPlusOffset, discreteness);

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                if (terrainMap[x, z] < 0.4f) { // FIXME: hardcoded treeline value
                    map[x, z] = 0;
                }
            }
        }

        return map;
    }
}
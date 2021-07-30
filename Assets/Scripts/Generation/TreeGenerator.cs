using UnityEngine;
using System;

public static class TreeGenerator {

    public static float[,] GenerateTreeMap(float[,] heightMap, System.Random seedRNG, float scale, int octaves, float persistance, float lacunarity, Vector2 centerPlusOffset, float discreteness) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float[,] treeDensityMap = Noise.GenerateNoiseMap(width, height, seedRNG, scale, octaves, persistance, lacunarity, centerPlusOffset, discreteness);

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                if (heightMap[x, z] < 0.4f) { // FIXME: hardcoded treeline value
                    treeDensityMap[x, z] = 0;
                }
            }
        }

        return treeDensityMap;
    }
}
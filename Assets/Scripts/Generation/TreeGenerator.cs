using UnityEngine;
using System.Collections.Generic;

public static class TreeGenerator {
    public static float[,] GenerateTreeMap(float[,] heightMap, System.Random seedRNG, float scale, int octaves, float persistance, float lacunarity, Vector2 centerPlusOffset, float waterHeight, float discreteness) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float waterPadding = 0.05f;

        float[,] treeDensityMap = Noise.GenerateNoiseMap(width, height, seedRNG, scale, octaves, persistance, lacunarity, centerPlusOffset, discreteness);

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                if (heightMap[x, z] < waterHeight + waterPadding) {
                    treeDensityMap[x, z] = 0;
                }
            }
        }

        return treeDensityMap;
    }

    public static void AddTreeData(ref List<ThingData> thingList, Vector3 position, Quaternion rotation, GameObject prefab) {
        thingList.Add(new ThingData(ThingType.Tree, prefab, position, rotation));
    }
}
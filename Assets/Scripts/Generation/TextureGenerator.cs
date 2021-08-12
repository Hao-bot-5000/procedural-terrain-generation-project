using UnityEngine;
using System.Collections.Generic;

public static class TextureGenerator {
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // 1D color mapping of 2D heightMap
        Color[] colorMap = new Color[width * height];
        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                // colorMap[(z * width) + x] = color of heightMap[x, z] - 
                //      z = num rows
                //      x = num cols
                colorMap[z * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, z]);
            }
        }
        
        return TextureFromColorMap(colorMap, width, height);
    }
}
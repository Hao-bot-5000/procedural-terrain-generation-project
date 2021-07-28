using UnityEngine;
using System.Collections.Generic;

public static class TextureGenerator {
    public static List<Texture2D> TexturesFromColorMap(Color[] colorMap, int chunkSize, int mapSize, int verticesPerSide) {
        chunkSize++;

        List<Texture2D> textureList = new List<Texture2D>();

        for (int i = 0; i < mapSize * mapSize; i++) {
            Color[] chunk = new Color[chunkSize * chunkSize];
            int offsetX = (i % mapSize) * (chunkSize - (i % mapSize != 0 ? 1 : 0));
            int offsetZ = (i / mapSize) * (chunkSize - (i >= mapSize ? 1 : 0));
            for (int z = 0; z < chunkSize; z++) {
                for (int x = 0; x < chunkSize; x++) {
                    // Debug.Log(i + " - [" + x + ", " + z + "] - " + (x + offsetX) + " " + (z + offsetZ) + " | Chunk: " + (z * chunkSize + x) + " | ColorMap: " + ((z + offsetZ) * verticesPerSide + (x + offsetX)));
                    chunk[z * chunkSize + x] = colorMap[(z + offsetZ) * verticesPerSide + (x + offsetX)];
                }
            }
            textureList.Add(TextureFromColorMap(chunk, chunkSize, chunkSize));
        }

        return textureList;
    }

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
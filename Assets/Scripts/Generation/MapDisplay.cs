using UnityEngine;
using System.Collections.Generic;

public class MapDisplay : MonoBehaviour {
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public MeshFilter waterFilter;

    public Material terrainMaterial;

    Dictionary<string, GameObject> children;
    
    public void DrawTexture(Texture2D texture) {
        meshFilter.sharedMesh = null;
        meshRenderer.sharedMaterial.mainTexture = null;
        meshCollider.sharedMesh = null;

        waterFilter.sharedMesh = null;

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(-texture.width, 1, texture.height);
    }

    public void DrawTerrainMesh(MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void DrawTerrainMeshes(List<MeshData> meshDataList, List<Texture2D> textureList, int chunkSize, int mapSize, float scale) {
        if (children == null) children = new Dictionary<string, GameObject>();
        foreach (Transform child in transform) {
            if (child.name.StartsWith("chunk")) children.Add(child.name, child.gameObject);
        }
        
        for (int i = 0; i < meshDataList.Count; i++) {
            bool chunkExists = children.ContainsKey("chunk (" + i + ")");

            GameObject chunkObject = chunkExists ? children["chunk (" + i + ")"] : new GameObject("chunk (" + i + ")");
            float topLeftX = -(chunkSize / 2f) * (mapSize - 1);
            float topLeftZ =  (chunkSize / 2f) * (mapSize - 1);
            Vector3 position = new Vector3(topLeftX + (i % mapSize) * chunkSize, 0, topLeftZ - ((i / mapSize)) * chunkSize);
            // Debug.Log(i + ": " + position);

            MeshRenderer chunkMeshRenderer = chunkExists ? chunkObject.GetComponent<MeshRenderer>() : chunkObject.AddComponent<MeshRenderer>();
            MeshFilter chunkMeshFilter = chunkExists ? chunkObject.GetComponent<MeshFilter>() : chunkObject.AddComponent<MeshFilter>();
            MeshCollider chunkMeshCollider = chunkExists ? chunkObject.GetComponent<MeshCollider>() : chunkObject.AddComponent<MeshCollider>();

            chunkMeshRenderer.sharedMaterial = terrainMaterial;
            chunkMeshRenderer.material.mainTexture = textureList[i];
            chunkMeshFilter.sharedMesh = meshDataList[i].CreateMesh();
            chunkMeshCollider.sharedMesh = chunkMeshFilter.sharedMesh;

            chunkObject.transform.position = position * scale;
            chunkObject.transform.parent = transform;
            chunkObject.transform.localScale = Vector3.one * scale;
        }
    }

    // NOTE: water
    public void DrawWaterMesh(MeshData meshData) {
        waterFilter.sharedMesh = meshData.CreateMesh();
    }

    public void DrawTrees(MeshData meshData, float[,] treeMap) {
        if (treeMap == null) return;

        // Generate tree meshes here :)
    }


    public void ClearDisplay() {
        meshFilter.sharedMesh = null;
        meshRenderer.sharedMaterial.mainTexture = null;
        meshCollider.sharedMesh = null;

        waterFilter.sharedMesh = null;

        textureRenderer.sharedMaterial.mainTexture = null;
        textureRenderer.transform.localScale = Vector3.one;
    }
}

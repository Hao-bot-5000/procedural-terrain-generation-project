using UnityEngine;
using System.Collections.Generic;

public class MapDisplay : MonoBehaviour {
    // Used for displaying other DrawMode types
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    public MeshFilter waterFilter;

    // Used for displaying Chunk DrawMode
    public Material landMaterial;
    public Material waterMaterial;
    public int chunkLayer;
    public int waterLayer;

    Dictionary<string, GameObject> children;
    
    public void DrawTexture(Texture2D texture) {
        meshFilter.sharedMesh = null;
        meshRenderer.sharedMaterial.mainTexture = null;
        meshCollider.sharedMesh = null;

        waterFilter.sharedMesh = null;

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(-texture.width, 1, texture.height);
    }

    public void DrawChunks(List<ChunkData> chunkDataList, /*int chunkSize, */int mapSize, float mapHeightMultiplier, float waterLevel) {
        if (children == null) children = new Dictionary<string, GameObject>();
        foreach (Transform child in transform) {
            if (child.name.StartsWith("chunk")) children.Add(child.name, child.gameObject);
        }

        for (int i = 0; i < chunkDataList.Count; i++) {
            bool chunkExists = children.ContainsKey("chunk (" + i + ")");
            ChunkData chunkData = chunkDataList[i];
            // Debug.Log(chunkSize + " | " + chunkData.size);

            GameObject chunkObject = chunkExists ? children["chunk (" + i + ")"] : new GameObject("chunk (" + i + ")");
            chunkObject.layer = chunkLayer;

            BoxCollider chunkTrigger = chunkObject.GetComponent<BoxCollider>() ? chunkObject.GetComponent<BoxCollider>() : chunkObject.AddComponent<BoxCollider>();

            float topLeftX = -(chunkData.size / 2f) * (mapSize - 1);
            float topLeftZ =  (chunkData.size / 2f) * (mapSize - 1);
            Vector3 position = new Vector3(topLeftX + (i % mapSize) * chunkData.size, 0, topLeftZ - ((i / mapSize)) * chunkData.size);
            
            chunkObject.transform.position = position * chunkData.scale;
            chunkObject.transform.parent = transform;

            float triggerPadding = 4f;
            chunkTrigger.center = Vector3.up * (mapHeightMultiplier + triggerPadding) * 0.5f * chunkData.scale;
            chunkTrigger.size = new Vector3(chunkData.size, mapHeightMultiplier + triggerPadding, chunkData.size) * chunkData.scale;
            chunkTrigger.isTrigger = true;

            GameObject landObject = chunkExists ? chunkObject.transform.GetChild(0).gameObject : new GameObject("land");

            MeshRenderer landMeshRenderer = landObject.GetComponent<MeshRenderer>() ? landObject.GetComponent<MeshRenderer>() : landObject.AddComponent<MeshRenderer>();
            MeshFilter landMeshFilter = landObject.GetComponent<MeshFilter>() ? landObject.GetComponent<MeshFilter>() : landObject.AddComponent<MeshFilter>();
            MeshCollider landMeshCollider = landObject.GetComponent<MeshCollider>() ? landObject.GetComponent<MeshCollider>() : landObject.AddComponent<MeshCollider>();

            // Debug.Log(landObject + " | " + landMeshRenderer + " | " + landObject.AddComponent<MeshRenderer>());
            // Debug.Log(chunkData.landTexture);
            landMeshRenderer.sharedMaterial = landMaterial;
            landMeshRenderer.material.mainTexture = chunkData.landTexture;
            landMeshFilter.sharedMesh = chunkData.landMeshData.CreateMesh();
            landMeshCollider.sharedMesh = landMeshFilter.sharedMesh;

            landObject.transform.position = position * chunkData.scale;
            landObject.transform.parent = chunkObject.transform;
            landObject.transform.localScale = Vector3.one * chunkData.scale;

            GameObject waterObject = chunkExists ? chunkObject.transform.GetChild(1).gameObject : new GameObject("water");
            waterObject.layer = waterLayer;

            MeshRenderer waterMeshRenderer = waterObject.GetComponent<MeshRenderer>() ? waterObject.GetComponent<MeshRenderer>() : waterObject.AddComponent<MeshRenderer>();
            MeshFilter waterMeshFilter = waterObject.GetComponent<MeshFilter>() ? waterObject.GetComponent<MeshFilter>() : waterObject.AddComponent<MeshFilter>();

            waterMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            waterMeshRenderer.sharedMaterial = waterMaterial;
            waterMeshFilter.sharedMesh = chunkData.waterMeshData.CreateMesh(isDynamic: true);

            waterObject.transform.position = (position + (Vector3.up * waterLevel)) * chunkData.scale;
            waterObject.transform.parent = chunkObject.transform;
            waterObject.transform.localScale = Vector3.one * chunkData.scale;
        }


    }

    public void DrawLandMesh(MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void DrawWaterMesh(MeshData meshData) {
        waterFilter.sharedMesh = meshData.CreateMesh(isDynamic: true);
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

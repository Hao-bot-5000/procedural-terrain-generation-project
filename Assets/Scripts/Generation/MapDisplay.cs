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
    public LayerMask waterMask;

    Dictionary<string, GameObject> children;
    
    public void DrawTexture(Texture2D texture) {
        meshFilter.sharedMesh = null;
        meshRenderer.sharedMaterial.mainTexture = null;
        meshCollider.sharedMesh = null;

        waterFilter.sharedMesh = null;

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(-texture.width, 1, texture.height);
    }

    public void DrawChunks(List<ChunkData> chunkDataList, int chunkSize, int mapSize, float waterLevel, float scale) {
        if (children == null) children = new Dictionary<string, GameObject>();
        foreach (Transform child in transform) {
            if (child.name.StartsWith("chunk")) children.Add(child.name, child.gameObject);
        }

        for (int i = 0; i < chunkDataList.Count; i++) {
            bool chunkExists = children.ContainsKey("chunk (" + i + ")");

            GameObject chunkObject = chunkExists ? children["chunk (" + i + ")"] : new GameObject("chunk (" + i + ")");
            float topLeftX = -(chunkSize / 2f) * (mapSize - 1);
            float topLeftZ =  (chunkSize / 2f) * (mapSize - 1);
            Vector3 position = new Vector3(topLeftX + (i % mapSize) * chunkSize, 0, topLeftZ - ((i / mapSize)) * chunkSize);
            
            chunkObject.transform.position = position * scale;
            chunkObject.transform.parent = transform;

            GameObject landObject = chunkExists ? chunkObject.transform.GetChild(0).gameObject : new GameObject("land");

            MeshRenderer landMeshRenderer = landObject.GetComponent<MeshRenderer>() ? landObject.GetComponent<MeshRenderer>() : landObject.AddComponent<MeshRenderer>();
            MeshFilter landMeshFilter = landObject.GetComponent<MeshFilter>() ? landObject.GetComponent<MeshFilter>() : landObject.AddComponent<MeshFilter>();
            MeshCollider landMeshCollider = landObject.GetComponent<MeshCollider>() ? landObject.GetComponent<MeshCollider>() : landObject.AddComponent<MeshCollider>();

            // Debug.Log(landObject + " | " + landMeshRenderer + " | " + landObject.AddComponent<MeshRenderer>());
            Debug.Log(chunkDataList[i].landTexture);
            landMeshRenderer.sharedMaterial = landMaterial;
            landMeshRenderer.material.mainTexture = chunkDataList[i].landTexture;
            landMeshFilter.sharedMesh = chunkDataList[i].landMeshData.CreateMesh();
            landMeshCollider.sharedMesh = landMeshFilter.sharedMesh;

            landObject.transform.position = position * scale;
            landObject.transform.parent = chunkObject.transform;
            landObject.transform.localScale = Vector3.one * scale;

            GameObject waterObject = chunkExists ? chunkObject.transform.GetChild(1).gameObject : new GameObject("water");
            waterObject.layer = waterMask;

            MeshRenderer waterMeshRenderer = waterObject.GetComponent<MeshRenderer>() ? waterObject.GetComponent<MeshRenderer>() : waterObject.AddComponent<MeshRenderer>();
            MeshFilter waterMeshFilter = waterObject.GetComponent<MeshFilter>() ? waterObject.GetComponent<MeshFilter>() : waterObject.AddComponent<MeshFilter>();
            TerrainMovement meshMovementScript = waterObject.GetComponent<TerrainMovement>() ? waterObject.GetComponent<TerrainMovement>() : waterObject.AddComponent<TerrainMovement>();
            // Could add code here to modify wave properties

            waterMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            waterMeshRenderer.sharedMaterial = waterMaterial;
            waterMeshFilter.sharedMesh = chunkDataList[i].waterMeshData.CreateMesh(isDynamic: true);

            waterObject.transform.position = (position + (Vector3.up * waterLevel)) * scale;
            waterObject.transform.parent = chunkObject.transform;
            waterObject.transform.localScale = Vector3.one * scale;
        }


    }

    public void DrawLandMesh(MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    // public void DrawLandMeshes(List<MeshData> meshDataList, List<Texture2D> textureList, int chunkSize, int mapSize, float scale) {
    //     if (children == null) children = new Dictionary<string, GameObject>();
    //     foreach (Transform child in transform) {
    //         if (child.name.StartsWith("chunk")) children.Add(child.name, child.gameObject);
    //     }
        
    //     for (int i = 0; i < meshDataList.Count; i++) {
    //         bool chunkExists = children.ContainsKey("chunk (" + i + ")");

    //         GameObject chunkObject = chunkExists ? children["chunk (" + i + ")"] : new GameObject("chunk (" + i + ")");
    //         float topLeftX = -(chunkSize / 2f) * (mapSize - 1);
    //         float topLeftZ =  (chunkSize / 2f) * (mapSize - 1);
    //         Vector3 position = new Vector3(topLeftX + (i % mapSize) * chunkSize, 0, topLeftZ - ((i / mapSize)) * chunkSize);
    //         // Debug.Log(i + ": " + position);

    //         MeshRenderer chunkMeshRenderer = chunkExists ? chunkObject.GetComponent<MeshRenderer>() : chunkObject.AddComponent<MeshRenderer>();
    //         MeshFilter chunkMeshFilter = chunkExists ? chunkObject.GetComponent<MeshFilter>() : chunkObject.AddComponent<MeshFilter>();
    //         MeshCollider chunkMeshCollider = chunkExists ? chunkObject.GetComponent<MeshCollider>() : chunkObject.AddComponent<MeshCollider>();

    //         chunkMeshRenderer.sharedMaterial = landMaterial;
    //         chunkMeshRenderer.material.mainTexture = textureList[i];
    //         chunkMeshFilter.sharedMesh = meshDataList[i].CreateMesh();
    //         chunkMeshCollider.sharedMesh = chunkMeshFilter.sharedMesh;

    //         chunkObject.transform.position = position * scale;
    //         chunkObject.transform.parent = transform;
    //         chunkObject.transform.localScale = Vector3.one * scale;
    //     }
    // }

    public void DrawWaterMesh(MeshData meshData) {
        waterFilter.sharedMesh = meshData.CreateMesh(isDynamic: true);
    }

    // public void DrawWaterMeshes(List<MeshData> meshDataList, int chunkSize, int mapSize, float waterLevel, float scale) {
    //     if (children == null) children = new Dictionary<string, GameObject>();
    //     foreach (Transform child in transform) {
    //         if (child.name.StartsWith("water")) children.Add(child.name, child.gameObject);
    //     }

    //     for (int i = 0; i < meshDataList.Count; i++) {
    //         bool chunkExists = children.ContainsKey("water (" + i + ")");

    //         GameObject chunkObject = chunkExists ? children["water (" + i + ")"] : new GameObject("water (" + i + ")");
    //         float topLeftX = -(chunkSize / 2f) * (mapSize - 1);
    //         float topLeftZ =  (chunkSize / 2f) * (mapSize - 1);
    //         Vector3 position = new Vector3(topLeftX + (i % mapSize) * chunkSize, waterLevel, topLeftZ - ((i / mapSize)) * chunkSize);
    //         // Debug.Log(i + ": " + position);

    //         MeshRenderer chunkMeshRenderer = chunkExists ? chunkObject.GetComponent<MeshRenderer>() : chunkObject.AddComponent<MeshRenderer>();
    //         MeshFilter chunkMeshFilter = chunkExists ? chunkObject.GetComponent<MeshFilter>() : chunkObject.AddComponent<MeshFilter>();
            
    //         if (!chunkExists) {
    //             TerrainMovement meshMovementScript = chunkObject.AddComponent<TerrainMovement>();
    //             // Could add code here to modify wave properties
    //         }

    //         chunkMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    //         chunkMeshRenderer.sharedMaterial = waterMaterial;
    //         chunkMeshFilter.sharedMesh = meshDataList[i].CreateMesh(isDynamic: true);

    //         chunkObject.transform.position = position * scale;
    //         chunkObject.transform.parent = transform;
    //         chunkObject.transform.localScale = Vector3.one * scale;
    //     }
    // }

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

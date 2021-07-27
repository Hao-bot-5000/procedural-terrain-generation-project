using UnityEngine;
using System.Collections.Generic;

public class MapDisplay : MonoBehaviour {
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public MeshFilter waterFilter;
    
    public void DrawTexture(Texture2D texture) {
        meshFilter.sharedMesh = null;
        meshRenderer.sharedMaterial.mainTexture = null;
        meshCollider.sharedMesh = null;

        waterFilter.sharedMesh = null;

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(-texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void DrawMeshes(List<MeshData> meshDataList, int chunkSize, int mapSize, float scale) {
        Dictionary<string, GameObject> children = new Dictionary<string, GameObject>();
        foreach (Transform child in transform) {
            if (child.name.StartsWith("chunk")) children.Add(child.name, child.gameObject);
        }
        
        int i = 0;
        foreach (MeshData meshData in meshDataList) {
            bool chunkExists = children.ContainsKey("chunk (" + i + ")");

            GameObject chunkObject = chunkExists ? children["chunk (" + i + ")"] : new GameObject("chunk (" + i + ")");
            int topLeftX = -2 * (mapSize - 1);
            int topLeftZ =  2 * (mapSize - 1);
            // FIXME: position is off-centered
            Vector3 position = new Vector3(topLeftX + (i % mapSize) * chunkSize, 0, topLeftZ - ((i / mapSize)) * chunkSize);
            Debug.Log(i + ": " + position);

            // FIXME: Preexisting chunks are not registering updates
            if (!chunkExists) {
                meshRenderer = chunkObject.AddComponent<MeshRenderer>();
                meshFilter = chunkObject.AddComponent<MeshFilter>();
                meshCollider = chunkObject.AddComponent<MeshCollider>();
            }

            // meshRenderer.material = material;
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshCollider.sharedMesh = meshFilter.sharedMesh;

            chunkObject.transform.position = position * scale;
            chunkObject.transform.parent = transform;
            chunkObject.transform.localScale = Vector3.one * scale;

            i++;
        }
    }

    // NOTE: water
    public void DrawWater(MeshData meshData) {
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

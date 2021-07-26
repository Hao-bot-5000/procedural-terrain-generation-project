using UnityEngine;

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

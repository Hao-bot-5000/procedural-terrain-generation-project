using UnityEngine;

public class PlayerVision : MonoBehaviour {
    public Transform playerCamera;
    public int chunkLayer;
    public int backgroundLayer;

    // TODO: underwater fog color needs to scale with time of day
    public Color underwaterFogColor = new Color(0.15f, 0.55f, 0.75f, 0.6f);
    public float underwaterFogDensity = 0.01f;

    Transform currentWaterTransform;
    // TerrainMovement currentWaterMovement;

    enum VisionMode { Default, Underwater }; // Might be useful if we ever add other vision effects?
    VisionMode visionMode = VisionMode.Default;

    // http://wiki.unity3d.com/index.php?title=Underwater_Script
    bool defaultFog;
    Color defaultFogColor;
    float defaultFogDensity;
    // Material defaultSkybox;
    // Material noSkybox;

    float waveLength;
    float waveSpeed;
    float waveHeight;

    void Start() {
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        // defaultSkybox = RenderSettings.skybox;

        UpdateCullDistances();
    }

    void Update() {
        // Only runs if player is below water level and vision mode has not been switched to type Underwater yet
        float currentWaterLevel = GetWaveHeightAtPosition(playerCamera.position);
        // Debug.Log(playerCamera.position + " | " + currentWaterLevel);
        if (playerCamera.position.y < currentWaterLevel && visionMode == VisionMode.Default) {
            RenderSettings.fog = true;
            RenderSettings.fogColor = underwaterFogColor;
            RenderSettings.fogDensity = underwaterFogDensity;

            visionMode = VisionMode.Underwater;
        }
        // Only runs if player is above water level and the vision mode has not been switched to type Default yet
        else if (playerCamera.position.y >= currentWaterLevel && visionMode == VisionMode.Underwater) {
            RenderSettings.fog = defaultFog;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;

            visionMode = VisionMode.Default;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == chunkLayer) {
            currentWaterTransform = other.transform.childCount > 1 ? other.transform.GetChild(1) : null; // NOTE: Assumes that water object is the 2nd child of chunk object
            if (currentWaterTransform != null) {
                MeshRenderer currentWaterMeshRenderer = currentWaterTransform.GetComponent<MeshRenderer>();
                waveLength = currentWaterMeshRenderer.sharedMaterial.GetFloat("_WaveLength");
                waveSpeed = currentWaterMeshRenderer.sharedMaterial.GetFloat("_WaveSpeed");
                waveHeight = currentWaterMeshRenderer.sharedMaterial.GetFloat("_WaveHeight");
            }
        }
    }

    // NOTE: temporary until I develop my own culling mechanic
    private void UpdateCullDistances() {
        Camera cam = playerCamera.GetComponent<Camera>();

        float[] cullDistances = cam.layerCullDistances;

        for (int i = 0; i < cullDistances.Length; i++) {
            // If layer is not considered a background, reduce cull distance
            if (i != backgroundLayer) {
                cullDistances[i] = cam.farClipPlane * 0.85f;
            }
        }

        cam.layerCullDistances = cullDistances;
        cam.layerCullSpherical = true;
    }

    private float GetWaveHeightAtPosition(Vector3 position) {
        // Calculate Gerstner wave movements
        float p = (position.x + position.z) / 16;
        float k = 2 * Mathf.PI / waveLength;
        float f = k * (p - waveSpeed * Shader.GetGlobalVector("_Time").y);

        return currentWaterTransform != null ? currentWaterTransform.position.y + waveHeight * Mathf.Sin(f) : 0;
    }
}

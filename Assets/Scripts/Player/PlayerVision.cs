using UnityEngine;

public class PlayerVision : MonoBehaviour {
    public Transform playerCamera;
    public DayNightCycle dayNightCycle;
    public int chunkLayer;
    public int backgroundLayer;

    public Color underwaterFogColor;
    public float underwaterFogDensity;

    Material defaultSkybox;
    bool defaultFog;
    Color defaultFogColor;
    float defaultFogDensity;

    Transform currentWaterTransform;

    enum VisionMode { Default, Underwater }; // Might be useful if we ever add other vision effects?
    VisionMode visionMode = VisionMode.Default;

    float waveLength;
    float waveSpeed;
    float waveHeight;

    Texture2D noiseTexture;

    Vector4 shaderTime;

    void Start() {
        defaultSkybox = RenderSettings.skybox;
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;

        UpdateCullDistances();
    }

    void Update() {
        shaderTime = Shader.GetGlobalVector("_Time");
        bool isAboveWater = playerCamera.position.y > GetWaveHeightAtPosition(playerCamera.position);

        if (isAboveWater && visionMode == VisionMode.Underwater) {
            // RenderSettings.skybox = defaultSkybox;
            RenderSettings.fog = defaultFog;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;
            
            visionMode = VisionMode.Default;
        }
        else if (!isAboveWater) {
            if (visionMode == VisionMode.Default) {
                // RenderSettings.skybox = null;
                RenderSettings.fog = true;
                RenderSettings.fogDensity = underwaterFogDensity;

                visionMode = VisionMode.Underwater;
            }
            RenderSettings.fogColor = underwaterFogColor * (dayNightCycle.intensity * 0.8f + 0.2f);
        }
    }

    // NOTE: I just realized this is kind of useless since every water mesh shares the same material...
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == chunkLayer) {
            currentWaterTransform = other.transform.childCount > 1 ? other.transform.GetChild(1) : null; // NOTE: Assumes that water object is the 2nd child of chunk object
            if (currentWaterTransform != null) {
                MeshRenderer currentWaterMeshRenderer = currentWaterTransform.GetComponent<MeshRenderer>();
                waveLength = currentWaterMeshRenderer.sharedMaterial.GetFloat("_WaveLength");
                waveSpeed = currentWaterMeshRenderer.sharedMaterial.GetFloat("_WaveSpeed");
                waveHeight = currentWaterMeshRenderer.sharedMaterial.GetFloat("_WaveHeight");

                noiseTexture = currentWaterMeshRenderer.sharedMaterial.GetTexture("_NoiseTex") as Texture2D;
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
        if (currentWaterTransform == null) return 0;

        // Calculate Gerstner wave movements
        float p = (position.x + position.z) / 16;
        float k = 2 * Mathf.PI / waveLength;
        float f = k * (p - waveSpeed * shaderTime.y);

        return currentWaterTransform.position.y + (waveHeight * Mathf.Sin(f)) + GetValueFromNoiseTexture(position);
    }

    private float GetValueFromNoiseTexture(Vector3 position) {
        float heightSample = (noiseTexture.GetPixel(Mathf.RoundToInt((position.x + shaderTime.x) / 512), Mathf.RoundToInt((position.z + shaderTime.z) / 512)).r * 2 - 1) * 4;
        // Debug.Log(heightSample);
        // return (tex2Dlod(_NoiseTex, float4(v0.xz + _Time.xz, 0, 0) / 512) * 2 - 1) * 4;
        return heightSample;
    }
}

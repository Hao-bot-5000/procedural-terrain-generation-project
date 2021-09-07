using UnityEngine;

public class PlayerVision : MonoBehaviour {
    public Transform playerCamera;
    public DayNightCycle dayNightCycle;

    public int backgroundLayer;

    public bool underwaterFog;
    public Color underwaterFogColor;
    public float underwaterFogDensity;

    PlayerStatus playerStatus;
    PlayerStatus.StatusType prevStatus;

    Material defaultSkybox;
    bool defaultFog;
    Color defaultFogColor;
    float defaultFogDensity;

    void Start() {
        playerStatus = GetComponent<PlayerStatus>();
        prevStatus = playerStatus.status;

        defaultSkybox = RenderSettings.skybox;
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;

        UpdateCullDistances();
    }

    void Update() {
        if (prevStatus != playerStatus.status) {
            switch (playerStatus.status) {
                case PlayerStatus.StatusType.Underwater:
                    RenderSettings.fog = underwaterFog;
                    RenderSettings.fogDensity = underwaterFogDensity;
                    break;
                default:
                    RenderSettings.fog = defaultFog;
                    RenderSettings.fogDensity = defaultFogDensity;
                    break;
            }

            prevStatus = playerStatus.status;
        }

        RenderSettings.fogColor = CalculateFogColor();
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

    private Color CalculateFogColor() {
        switch (prevStatus) {
            case PlayerStatus.StatusType.Underwater:
                return underwaterFogColor * (dayNightCycle.intensity * 0.8f + 0.2f);
            default:
                return defaultFogColor * dayNightCycle.intensity;
        }
    }
}

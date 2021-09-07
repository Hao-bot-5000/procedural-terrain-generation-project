using UnityEngine;

public class PlayerStatus : MonoBehaviour {
    public enum StatusType { Default, Underwater }; // Might be useful if we ever add other vision effects?
    [SerializeField] private StatusType _status = StatusType.Default;
    public StatusType status { get { return _status; } }
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private int chunkLayer;

    private Vector4 _shaderTime;
    private Transform _currentWaterTransform;
    private Renderer _currentMeshRenderer;
    private float _currentWaveLength;
    private float _currentWaveSpeed;
    private float _currentWaveHeight;
    private Texture2D _currentNoiseTexture;

    void Update() {
        _shaderTime = Shader.GetGlobalVector("_Time");
        bool isAboveWater = _playerCamera.position.y > GetWaveHeightAtPosition(_playerCamera.position);

        if (isAboveWater && _status == StatusType.Underwater) {
            _status = StatusType.Default;
        }
        else if (!isAboveWater && _status == StatusType.Default) {
            _status = StatusType.Underwater;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == chunkLayer) {
            _currentWaterTransform = other.transform.childCount > 1 ? other.transform.GetChild(1) : null; // NOTE: Assumes that water object is the 2nd child of chunk object
            if (_currentWaterTransform != null) {
                _currentMeshRenderer = _currentWaterTransform.GetComponent<MeshRenderer>();
                _currentWaveLength = _currentMeshRenderer.sharedMaterial.GetFloat("_WaveLength");
                _currentWaveSpeed = _currentMeshRenderer.sharedMaterial.GetFloat("_WaveSpeed");
                _currentWaveHeight = _currentMeshRenderer.sharedMaterial.GetFloat("_WaveHeight");

                _currentNoiseTexture = _currentMeshRenderer.sharedMaterial.GetTexture("_NoiseTex") as Texture2D;
            }
        }
    }

    private float GetWaveHeightAtPosition(Vector3 position) {
        if (_currentWaterTransform == null) return 0;

        // Calculate Gerstner wave movements
        float p = (position.x + position.z) / 16;
        float k = 2 * Mathf.PI / _currentWaveLength;
        float f = k * (p - _currentWaveSpeed * _shaderTime.y);

        return _currentWaterTransform.position.y + (_currentWaveHeight * Mathf.Sin(f)) + GetValueFromNoiseTexture(position);
    }

    private float GetValueFromNoiseTexture(Vector3 position) {
        float heightSample = (_currentNoiseTexture.GetPixel(Mathf.RoundToInt((position.x + _shaderTime.x) / 512), Mathf.RoundToInt((position.z + _shaderTime.z) / 512)).r * 2 - 1) * 4;
        return heightSample;
    }
}

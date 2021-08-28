using UnityEngine;

public class MoonModule : DayNightModule {
    [SerializeField] private Transform _moonTransform;
    private Light _moonLight;
    [SerializeField] private Gradient _moonColor;
    [SerializeField] private float _baseIntensity;
    [SerializeField] private float _intensityVariation;

    void Start() {
        _moonLight = _moonTransform.GetComponent<Light>();
    }

    public override void UpdateModule(float intensity) {
        _moonLight.intensity = (1 - intensity) * _intensityVariation + _baseIntensity;

        _moonLight.color = _moonColor.Evaluate(1 - intensity);
    }
}

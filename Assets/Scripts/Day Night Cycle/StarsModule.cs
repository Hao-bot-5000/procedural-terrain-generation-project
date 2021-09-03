using UnityEngine;

public class StarsModule : DayNightModule {
    [SerializeField] private Renderer _starsParticleRenderer;
    // [SerializeField] private float _baseIntensity;
    // [SerializeField] private float _intensityVariation;
    [SerializeField] private AnimationCurve _intensityCurve;
    private Color _starsMaterialColor;

    public override void UpdateModule(float intensity) {
        // _starsMaterialColor = _starsParticleRenderer.material.color;
        _starsMaterialColor =  _starsParticleRenderer.material.GetColor("_TintColor");
        _starsMaterialColor.a = _intensityCurve.Evaluate(intensity); // (1 - intensity) * _intensityVariation + _baseIntensity;
        // _starsParticleRenderer.material.color = _starsMaterialColor;
        _starsParticleRenderer.material.SetColor("_TintColor", _starsMaterialColor);
    }
}

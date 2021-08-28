using UnityEngine;
using System.Collections.Generic;

public class DayNightCycle : MonoBehaviour {
    [Space(10)]
    public bool isPaused = false;

    [Header("Data")]
    [SerializeField] [Range(0, 1)] private float _timeOfDay;
    public float timeOfDay { get { return _timeOfDay; } }
    [SerializeField] private int _dayLength = 30; // In seconds
    public int dayLength { get { return _dayLength; } }
    [SerializeField] private int _yearLength = 3; // In days
    public int yearLength { get { return _yearLength; } }
    [SerializeField] private int _dayNumber;
    public int dayNumber { get { return _dayNumber; } }
    [SerializeField] private int _yearNumber;
    public int yearNumber { get { return _yearNumber; } }

    private float _timeScale = 100f;

    [Header("Sky")]
    [SerializeField] private Transform _skyTransform;
    private float _intensity;

    [Header("Sun")]
    [SerializeField] private Transform _sunTransform;
    private Light _sunLight;
    [SerializeField] private Gradient _sunColor;
    [SerializeField] private float _sunBaseIntensity = 0.5f;
    [SerializeField] private float _sunIntensityVariation = 0.5f;
    // private float _sunIntensity;
    // private float _sunProjectionValue;

    [Header("Ambience")]
    [SerializeField] private float _ambientBaseIntensity = 0.25f;
    [SerializeField] private float _ambientIntensityVariation = 0.75f;
    // private float _ambientIntensity;

    [Space(10)]
    [SerializeField] private List<DayNightModule> modules = new List<DayNightModule>();

    private float updateEverySecond = 0f;

    void Start() {
        _sunLight = _sunTransform.GetComponent<Light>();

        // NOTE: Starting position of sunlight seems to affect the land meshes' ambient lighting (sunlight with -90d rotation causes land meshes 
        //       to be pitch black at night time, sunlight with 90d rotation causes land meshes to have dim ambient lighting at night time) 
        _sunTransform.localEulerAngles = Vector3.left * 90f;
    }

    void Update() {
        if (!isPaused) {
            UpdateTimeScale();
            UpdateTime();
        }

        AdjustSkyRotation();
        AdjustSkyLights();

        if ((updateEverySecond += Time.deltaTime) >= 1) {
            // Debug.Log("updating...");
            UpdateModules();

            updateEverySecond %= 1;
        }
    }

    private void UpdateTimeScale() {
        _timeScale = 24f / (_dayLength / 60f);
    }

    public void UpdateTime() {
        _timeOfDay += Time.deltaTime * _timeScale / 1440;

        // New day
        if (_timeOfDay >= 1) {
            _timeOfDay -= 1;
            _dayNumber++;
        }

        // New year
        if (_dayNumber >= _yearLength) {
            _dayNumber = 0;
            _yearNumber++;
        }
    }

    private void AdjustSkyRotation() {
        float sunAngle = timeOfDay * 360f;
        _skyTransform.localEulerAngles = Vector3.forward * sunAngle;
    }

    private void AdjustSkyLights() {
        _intensity = Vector3.Dot(_sunLight.transform.forward, Vector3.down);
        _intensity = Mathf.Clamp01(_intensity);

        AdjustSunLight();
        AdjustAmbientLight();
    }

    private void AdjustSunLight() {
        // Adjust light intensity
        // _sunIntensity = Mathf.Clamp01(_sunProjectionValue);
        _sunLight.intensity = _intensity * _sunIntensityVariation + _sunBaseIntensity;
    
        // Adjust light color
        _sunLight.color = _sunColor.Evaluate(_intensity);
    }

    private void AdjustAmbientLight() {
        // _ambientIntensity = Mathf.Clamp01(_sunProjectionValue);
        RenderSettings.ambientIntensity = _intensity * _ambientIntensityVariation + _ambientBaseIntensity;
    }

    public void AddModule(DayNightModule module) {
        modules.Add(module);
    }

    public void RemoveModule(DayNightModule module) {
        modules.Remove(module);
    }

    private void UpdateModules() {
        foreach (DayNightModule module in modules) {
            module.UpdateModule(_intensity);
        }
    }

    // public float secondsPerCycle = 1200f;

    // public Light sun;
    // public Light moon;

    // public AnimationCurve sunIntensity;
    // public AnimationCurve moonIntensity;

    // public AnimationCurve ambientReflectionIntensity;

    // float deltaDegreesPerSecond;
    // float timeCycle;

    // void Update() {
    //     deltaDegreesPerSecond = 360f / secondsPerCycle;
    //     timeCycle = transform.localEulerAngles.z / 360f;

    //     transform.RotateAround(Vector3.zero, Vector3.forward, deltaDegreesPerSecond * Time.deltaTime);
    //     transform.LookAt(Vector3.zero);

    //     // Debug.Log(timeCycle);
    //     sun.intensity = sunIntensity.Evaluate(timeCycle);
    //     moon.intensity = moonIntensity.Evaluate(timeCycle);

    //     RenderSettings.ambientIntensity = ambientReflectionIntensity.Evaluate(timeCycle);
    //     RenderSettings.reflectionIntensity = ambientReflectionIntensity.Evaluate(timeCycle);
    // }
}

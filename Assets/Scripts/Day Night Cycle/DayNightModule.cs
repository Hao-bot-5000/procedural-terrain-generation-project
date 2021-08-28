using UnityEngine;

public abstract class DayNightModule : MonoBehaviour {
    protected DayNightCycle dayNightControl;

    void OnEnable() {
        dayNightControl = GetComponent<DayNightCycle>();

        if (dayNightControl != null) {
            dayNightControl.AddModule(this);
        }
    }

    void OnDisable() {
        if (dayNightControl != null) {
            dayNightControl.RemoveModule(this);
        }
    }

    public abstract void UpdateModule(float intensity);
}

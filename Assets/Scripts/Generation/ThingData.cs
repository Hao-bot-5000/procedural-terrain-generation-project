using UnityEngine;

public class ThingData {
    public ThingType type;
    public GameObject prefab { get; }

    public Vector3 position { get; }

    public ThingData(ThingType type, GameObject prefab, Vector3 position) {
        this.type = type;
        this.prefab = prefab;
        this.position = position;
    }
}

public enum ThingType {
    Tree
}
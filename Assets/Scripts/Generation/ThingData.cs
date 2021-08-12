using UnityEngine;

public class ThingData {
    public ThingType type { get; }
    public GameObject prefab { get; }

    public Vector3 position { get; }
    public Quaternion rotation { get; }

    public ThingData(ThingType type, GameObject prefab, Vector3 position, Quaternion rotation) {
        this.type = type;
        this.prefab = prefab;
        this.position = position;
        this.rotation = rotation;
    }
}

public enum ThingType {
    Tree
}
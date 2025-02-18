using System;
using UnityEngine;

public class Collectible : MonoBehaviour {
    [NonSerialized] public Beacon beacon;

    void Start() {
        FindObjectOfType<CollectibleHint>().Add(gameObject);
    }

    public void Collect() {
        FindObjectOfType<CollectibleHint>().Remove(gameObject);
        beacon.Collect();
        Destroy(gameObject);
    }
}

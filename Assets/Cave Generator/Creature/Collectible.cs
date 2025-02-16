using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour {
    void Start() {
        FindObjectOfType<CollectibleHint>().Add(this);
    }

    public void Collect() {
        FindObjectOfType<CollectibleHint>().Remove(this);
        Destroy(gameObject);
    }
}

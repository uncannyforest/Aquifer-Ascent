using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleHint : MonoBehaviour {
    public LineRenderer hint;
    public Collectible c;

    public void Add(Collectible c) {
        this.c = c;
        hint.enabled = true;
    }

    public void Remove(Collectible c) {
        if (this.c == c) {
            c = null;
            hint.enabled = false;
        }
    }

    void Update() {
        if (c != null) {
            hint.SetPositions(new Vector3[] {
                Vector3.zero,
                transform.InverseTransformPoint(c.transform.position)
            });
        }
    }
}

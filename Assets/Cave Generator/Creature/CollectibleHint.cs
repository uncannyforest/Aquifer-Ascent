using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleHint : MonoBehaviour {
    public LineRenderer hint;
    public GameObject target;

    public void Add(GameObject c) {
        target = c;
        hint.enabled = true;
    }

    public void Remove(GameObject c) {
        if (target == c) {
            c = null;
            hint.enabled = false;
        }
    }

    void Update() {
        if (target != null) {
            hint.SetPositions(new Vector3[] {
                Vector3.zero,
                transform.InverseTransformPoint(target.transform.position)
            });
        }
    }
}

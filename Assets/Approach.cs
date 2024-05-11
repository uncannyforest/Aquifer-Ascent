using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Approach : MonoBehaviour {
    public Transform target;
    public float rate = 1/6f;

    void Update() {
        transform.position = Vector3.Lerp(transform.position, target.position, rate * Time.deltaTime);
    }
}

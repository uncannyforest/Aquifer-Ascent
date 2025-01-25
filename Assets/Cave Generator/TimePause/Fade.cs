using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Fade : MonoBehaviour {
    public float totalTime = 1;

    private float endTime;
    private Material material;
    private Color color;

    void Start() {
        endTime = Time.time + totalTime;
        Renderer lr = GetComponent<Renderer>();
        material = new Material(lr.material);
        color = material.color;
        material.color = Color.black;
        lr.material = material;
    }

    void Update() {
        if (Time.time >= endTime) {
            Destroy(gameObject);
            return;
        }
        float time = (endTime - Time.time) / totalTime;
        Color color = Color.Lerp(Color.black, this.color, Quadratic(time));
        material.color = color;
    }

    private float Quadratic(float t) {
        return 4 * t * (1 - t);
    }
}

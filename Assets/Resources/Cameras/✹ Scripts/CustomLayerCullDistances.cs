using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CustomLayerCullDistances : MonoBehaviour {
    public LayerMask layers;
    public int cullDistance;

    void Start() => SetCullDistances();

    void OnValidate() => SetCullDistances();

    private void SetCullDistances() {
        Camera camera = GetComponent<Camera>();
        float[] cullDistances = camera.layerCullDistances;
        for (int i = 0; i < cullDistances.Length; i++) {
            if (layers.Contains(i)) cullDistances[i] = cullDistance;
        }
        camera.layerCullDistances = cullDistances;
        camera.layerCullSpherical = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraForceZone : MonoBehaviour {
    public Camera rackCamera;
    public float transitionTime = 2f;
    public bool defaultIsOrtho = false;
    public ScarfRack scarfRack;

    private CameraMux cameraMux;

    void Start() {
        cameraMux = GameObject.FindObjectOfType<CameraMux>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            cameraMux.SwitchCamera(rackCamera, rackCamera.transform.parent.gameObject, transitionTime);
            scarfRack.AddScarf();
            scarfRack.ReadInput = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            cameraMux.SwitchCamera(defaultIsOrtho, transitionTime);
            scarfRack.ReadInput = false;
        }
    }
}

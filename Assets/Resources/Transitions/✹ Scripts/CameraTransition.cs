
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraTransition : MonoBehaviour {
    public bool ortho;

    private GameObject player;
    private CameraMux cameraMux;

    void Start() {
        player = GameObject.FindWithTag("Player");
        cameraMux = GameObject.FindObjectOfType<CameraMux>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == player) {
            cameraMux.SwitchCamera(ortho);
        }
    }
}

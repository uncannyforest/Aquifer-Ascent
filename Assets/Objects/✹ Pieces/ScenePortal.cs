
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour {
    public string otherScene;
    public bool open;

    private GameObject player;
    private GameLoader gameLoader;

    void Start() {
        player = GameObject.FindWithTag("Player");
        gameLoader = GameObject.FindObjectOfType<GameLoader>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == player) {
            if (open) {
                LoadScene();
            } else {
                UnloadScene();
            }
        }
    }

    private void LoadScene() {
        StartCoroutine(gameLoader.EnsureSceneLoaded(otherScene));
    }
 
    private void UnloadScene() {
        SceneManager.SetActiveScene(gameObject.scene);
        
        gameLoader.EnsureSceneUnloaded(otherScene);
    }
}

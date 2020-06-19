
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    public string otherScene;
    public bool open;

    private GameObject player;

    void Start() {
        player = GameObject.FindWithTag("Player");
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

    void LoadScene() {
        if (!SceneManager.GetSceneByName(otherScene).isLoaded) {
            SceneManager.LoadSceneAsync(otherScene, LoadSceneMode.Additive);
        }
    }
 
    void UnloadScene() {
        SceneManager.SetActiveScene(gameObject.scene);
        
        if (SceneManager.GetSceneByName(otherScene).isLoaded) {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(otherScene));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.ThirdPerson;

public class GameManager : MonoBehaviour {
    private static GameManager instance;
    public static GameManager I { get => instance; }
    GameManager(): base() {
        instance = this;
    }

    public string startScene;

    public List<GameObject> deactivateTheseOnDeath = new List<GameObject>();

    private int mode = 1;

    void Start() {
        StartCoroutine(LoadScenes());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            switch (mode) {
                case 1: Die(); break;
                case 2: Restart(); break;
            }
        }
    }

    public void Die() {
        if (mode != 1) return;
        mode = 2;
        ThirdPersonUserControl playerControl = GameObject.FindObjectOfType<ThirdPersonUserControl>();
        GameObject.FindObjectOfType<CameraMux>().SwitchCameraNow(true);
        GameObject.FindObjectOfType<PanController>().transform.position = playerControl.transform.position;
        playerControl.enabled = false;
        foreach (GameObject go in deactivateTheseOnDeath) {
            go.SetActive(false);
        }
    }

    private void Restart() {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(startScene));
        if (mode != 2) return;
        mode = 1;
        Transform player = GameObject.FindWithTag("Player").transform;
        player.position = Vector3.zero;
        player.rotation = Quaternion.identity;
        player.GetComponent<ThirdPersonUserControl>().enabled = true;
        foreach (GameObject go in deactivateTheseOnDeath) {
            go.SetActive(true);
        }
        GameObject.FindObjectOfType<CameraMux>().SwitchCameraNow(false);
        StartCoroutine(LoadScenes());
    }

	IEnumerator LoadScenes() {
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
		yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(startScene));
	}

    
}

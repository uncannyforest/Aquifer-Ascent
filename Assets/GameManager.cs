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

    public GameObject pauseMenu;
    public GameObject diedMenu;

    public List<GameObject> activateTheseOnDeath = new List<GameObject>();
    public List<GameObject> deactivateTheseOnDeath = new List<GameObject>();

    private Mode mode = Mode.PLAYING;

    private enum Mode {
        PLAYING,
        PAUSED,
        DEAD_MENU,
        VIEWING_MAP
    }

    private ThirdPersonUserControl playerControl;

    void Start() {
        playerControl = GameObject.FindObjectOfType<ThirdPersonUserControl>();
        StartCoroutine(LoadScenes());
        Time.timeScale = 1;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            switch (mode) {
                case Mode.PLAYING: Pause(); break;
                case Mode.PAUSED: Unpause(); break;
                case Mode.DEAD_MENU: Restart(); break;
                case Mode.VIEWING_MAP: Die(); break;
            }
        }
    }

    public void Pause() {
        mode = Mode.PAUSED;
        playerControl.enabled = false;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }

    public void Unpause() {
        mode = Mode.PLAYING;
        playerControl.enabled = true;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void Die() {
        mode = Mode.DEAD_MENU;
        playerControl.enabled = false;
        Time.timeScale = 0;
        diedMenu.SetActive(true);
    }

    public void SeeMap() {
        mode = Mode.VIEWING_MAP;
        GameObject.FindObjectOfType<CameraMux>().SwitchCameraNow(true);
        GameObject.FindObjectOfType<PanController>().transform.position = playerControl.transform.position;
        foreach (GameObject go in deactivateTheseOnDeath) go.SetActive(false);
        foreach (GameObject go in activateTheseOnDeath) go.SetActive(true);
    }

    public void Restart() {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(startScene));
        mode = Mode.PLAYING;
        Transform player = playerControl.transform;
        player.position = Vector3.zero;
        player.rotation = Quaternion.identity;
        player.GetComponent<ThirdPersonUserControl>().enabled = true;
        foreach (GameObject go in deactivateTheseOnDeath) go.SetActive(true);
        foreach (GameObject go in activateTheseOnDeath) go.SetActive(false);
        Time.timeScale = 1;
        GameObject.FindObjectOfType<CameraMux>().SwitchCameraNow(false);
        StartCoroutine(LoadScenes());
    }

	IEnumerator LoadScenes() {
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
		yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(startScene));
	}

    
}

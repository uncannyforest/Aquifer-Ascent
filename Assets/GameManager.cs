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

    public List<GameObject> spawnBeforeLevelLoad = new List<GameObject>();

    public GameObject background;
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject diedMenu;
    public GameObject panCamera;

    public List<GameObject> activateTheseOnDeath = new List<GameObject>();
    public List<GameObject> deactivateTheseOnDeath = new List<GameObject>();

    private Mode mode = Mode.PLAYING;

    private enum Mode {
        FOYER,
        PLAYING,
        PAUSED,
        DEAD_MENU,
        VIEWING_MAP
    }

    private ThirdPersonUserControl playerControl;

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

    public void MainMenu() {
        mode = Mode.FOYER;
        playerControl.enabled = false;
        Time.timeScale = 0;
        diedMenu.SetActive(false);
        background.SetActive(true);
        mainMenu.SetActive(true);
        Scene maybeLevel = SceneManager.GetSceneByName(startScene);
        if (maybeLevel != null) SceneManager.UnloadSceneAsync(maybeLevel);
    }

    public void CloseMainMenu() {
        background.SetActive(false);
        mainMenu.SetActive(false);
    }

    public void Pause() {
        mode = Mode.PAUSED;
        playerControl.enabled = false;
        Time.timeScale = 0;
        AudioListener.pause = true;
        pauseMenu.SetActive(true);
    }

    public void Unpause() {
        mode = Mode.PLAYING;
        playerControl.enabled = true;
        Time.timeScale = 1;
        AudioListener.pause = false;
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
        GameObject.Destroy(GameObject.FindObjectOfType<UnityStandardAssets.Cameras.MixedAutoCam>().gameObject);
        panCamera.transform.position = playerControl.transform.position;
        foreach (GameObject go in deactivateTheseOnDeath) go.SetActive(false);
        foreach (GameObject go in activateTheseOnDeath) go.SetActive(true);
    }

    public void StartLevel(Random.State? seed = null) => StartCoroutine(LoadScenes(seed));

    public void RestartWithSeed(Random.State seed) => Restart(seed);
    public void Restart(Random.State? seed = null) {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(startScene));
        StartCoroutine(LoadScenes(seed));
    }

	IEnumerator LoadScenes(Random.State? seed = null) {
		yield return null; // Wait for old scene to unload
        List<GameObject> spawnedBeforeLevelLoad = new List<GameObject>();
        foreach (GameObject prefab in spawnBeforeLevelLoad) spawnedBeforeLevelLoad.Add(GameObject.Instantiate(prefab, null));
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
        Debug.Log("Loaded scene - seed? " + seed);
        if (seed is Random.State actualSeed) Random.state = actualSeed;
		yield return null; // Wait for new scene to load
        Scene levelScene = SceneManager.GetSceneByName(startScene);
		SceneManager.SetActiveScene(levelScene);
        foreach (GameObject go in spawnedBeforeLevelLoad) SceneManager.MoveGameObjectToScene(go, levelScene);
        playerControl = GameObject.FindObjectOfType<ThirdPersonUserControl>();
        Unpause();
    }
}

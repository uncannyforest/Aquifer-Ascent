using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.ThirdPerson;
using Random = UnityEngine.Random;

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
    public GameObject loadMenu;
    public GameObject saveMenu;
    public GameObject panCamera;

    public List<GameObject> deactivateOnLife = new List<GameObject>();
    public List<GameObject> deactivateOnMainMenu = new List<GameObject>();

    private Mode mode = Mode.FOYER;

    public enum Mode {
        FOYER,
        LOAD,
        PLAYING,
        PAUSED,
        DEAD_MENU,
        VIEWING_MAP,
        SAVE
    }

    private Dictionary<Mode, GameObject> menus;

    private ThirdPersonUserControl playerControl;
    private float lodBias = 20;

    public int initSeed;

    void Start() {
        menus = new Dictionary<Mode, GameObject> {
            [Mode.FOYER] = mainMenu,
            [Mode.LOAD] = loadMenu,
            [Mode.PAUSED] = pauseMenu,
            [Mode.DEAD_MENU] = diedMenu,
            [Mode.SAVE] = saveMenu,
        };
        EnterFoyer();
        lodBias = QualitySettings.lodBias;
    }

    void Update() {
        if (SimpleInput.GetButtonDown("Cancel")) {
            switch (mode) {
                case Mode.PLAYING: Pause(); break;
                case Mode.PAUSED: Unpause(); break;
                case Mode.VIEWING_MAP: Die(); break;
            }
        }
    }

    public void SwitchMenu(Mode newMode) {
        bool menuIsDisplayed = menus.TryGetValue(mode, out GameObject oldMenu);
        if (menuIsDisplayed) oldMenu.SetActive(false);
        mode = newMode;
        menuIsDisplayed = menus.TryGetValue(mode, out GameObject newMenu);
        if (menuIsDisplayed) newMenu.SetActive(true);
    }

    public void LeaveFoyer() {
        background.SetActive(false);
    }

    private void Play() {
        playerControl.enabled = true;
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    private void Unplay() {
        playerControl.enabled = false;
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void EnterFoyer() {
        SwitchMenu(Mode.FOYER);
        background.SetActive(true);
        if (playerControl != null) playerControl.enabled = false;
        Time.timeScale = 0;
        Scene maybeLevel = SceneManager.GetSceneByName(startScene);
        if (maybeLevel != null && maybeLevel.IsValid()) SceneManager.UnloadSceneAsync(maybeLevel);
        foreach (GameObject go in deactivateOnLife) go.SetActive(true);
        foreach (GameObject go in deactivateOnMainMenu) go.SetActive(false);
    }

    public void Pause() {
        SwitchMenu(Mode.PAUSED);
        Unplay();
    }

    public void Unpause() {
        SwitchMenu(Mode.PLAYING);
        Play();
    }

    public void Die() {
        SwitchMenu(Mode.DEAD_MENU);
        Unplay();
    }

    public void SeeMap() {
        SwitchMenu(Mode.VIEWING_MAP);
        GameObject.Destroy(GameObject.FindObjectOfType<UnityStandardAssets.Cameras.MixedAutoCam>().gameObject);
        panCamera.transform.position = playerControl.transform.position + Vector3.up * 2;
        foreach (GameObject go in deactivateOnLife) go.SetActive(true);
        QualitySettings.lodBias = float.MaxValue;
    }

    public void StartLevel(int? seed = null) => StartCoroutine(LoadScenes(seed));

    public void RestartWithSeed(int seed) => Restart(seed);
    public void Restart(int? seed = null) {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(startScene));
        StartCoroutine(LoadScenes(seed));
    }

	IEnumerator LoadScenes(int? seed = null) {
		yield return null; // Wait for old scene to unload
        List<GameObject> spawnedBeforeLevelLoad = new List<GameObject>();
        foreach (GameObject prefab in spawnBeforeLevelLoad) spawnedBeforeLevelLoad.Add(GameObject.Instantiate(prefab, null));
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
        Debug.Log("Loaded scene - seed? " + seed);
        if (seed is int actualSeed) initSeed = actualSeed;
        else initSeed = (int)DateTime.Now.Ticks;
        Random.InitState(initSeed);
		yield return null; // Wait for new scene to load
        Scene levelScene = SceneManager.GetSceneByName(startScene);
		SceneManager.SetActiveScene(levelScene);
        foreach (GameObject go in spawnedBeforeLevelLoad) SceneManager.MoveGameObjectToScene(go, levelScene);
        playerControl = GameObject.FindObjectOfType<ThirdPersonUserControl>();
        foreach (GameObject go in deactivateOnMainMenu) go.SetActive(true);
        foreach (GameObject go in deactivateOnLife) go.SetActive(false);
        QualitySettings.lodBias = lodBias;
        Unpause();
    }
}

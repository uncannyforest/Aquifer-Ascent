using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour {
    public string startScene;

    private List<State> loadedState = new List<State>();

    private StateManager stateManager;
    private FileManager fileManager;

    void Start() {
        stateManager = new StateManager(GameObject.FindObjectOfType<GuidManager>());
        fileManager = new FileManager();

        StartCoroutine(LoadNewGame());
    }

	IEnumerator LoadNewGame() {
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
		yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(startScene));
	}

    public void Register(State statefulObject) => loadedState.Add(statefulObject);

    public void Unregister(State statefulObject) => loadedState.Remove(statefulObject);

    public IEnumerator EnsureSceneLoaded(string scene) {
        if (!SceneManager.GetSceneByName(scene).isLoaded) {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            yield return null;
            yield return null;
            stateManager.LoadScene(scene, this);
        }
    }

    public void EnsureSceneUnloaded(string scene) {
        if (SceneManager.GetSceneByName(scene).isLoaded) {
            stateManager.UnloadScene(scene, loadedState);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(scene));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager {

    private Dictionary<string, List<State.Data>> persistentState
            = new Dictionary<string, List<State.Data>>();


    private GuidManager guidManager;

    public StateManager(GuidManager guidManager) {
        this.guidManager = guidManager;
    }

    public void LoadScene(string scene, GameLoader gameLoader) {
        if (persistentState.ContainsKey(scene)) {
            foreach (State.Data data in persistentState[scene]) {
                gameLoader.StartCoroutine(State.LoadObject(data, guidManager));
            }
        }
    }

    public void UnloadScene(string scene, List<State> loadedState) {
        List<State.Data> sceneState = new List<State.Data>();

        for (int i = loadedState.Count - 1; i >= 0; i--) {
            State loadedObject = loadedState[i];
            if (loadedObject.gameObject.scene.name == scene) {
                sceneState.Add(loadedObject.SaveObject());
                loadedState.RemoveAt(i);
            }
        }

        persistentState[scene] = sceneState;
    }

    public bool IncludesScene(string scene) {
        return persistentState.ContainsKey(scene);
    }
}

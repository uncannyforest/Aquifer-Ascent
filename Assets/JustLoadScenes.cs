using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JustLoadScenes : MonoBehaviour {
    public string startScene;

    void Start() {
        StartCoroutine(LoadScenes());
    }

	IEnumerator LoadScenes() {
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
		yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(startScene));
	}
}

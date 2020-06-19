using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public string startScene;

    void Start() {
        StartCoroutine(LoadStartScene());
    }

	IEnumerator LoadStartScene() {
        SceneManager.LoadScene(startScene, LoadSceneMode.Additive);
		yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(startScene));
	}
}

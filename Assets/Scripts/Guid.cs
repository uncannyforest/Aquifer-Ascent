using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
public class Guid : MonoBehaviour {

    public string id;

    private GuidManager manager;

    void Awake() {
        manager = GameObject.FindObjectOfType<GuidManager>();

        if (manager != null && (id == null || id == "" || manager.IsRegisteredAlready(this))) {
            if (Application.isPlaying) {
                Debug.LogError("Bad Guid state for " + gameObject.name + ": " + id);
            }
            Debug.LogWarning("New ID for " + gameObject.name + " replacing " + id);
            id = System.Guid.NewGuid().ToString();
            // The following lines tell Unity to let the developer save the change.
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        manager.Register(this);
    }

    void OnDestroy() {
        manager.Unregister(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Guid : MonoBehaviour {

    public string id;

    private GuidManager manager;

    void Awake() {
        manager = GameObject.FindObjectOfType<GuidManager>();
        if (manager == null) return;

        if (manager != null && (id == null || id == "" || manager.IsRegisteredAlready(this))) {
            if (Application.isPlaying) {
                Debug.LogError("Bad Guid state for " + gameObject.name + ": " + id);
            }
            Debug.LogWarning("New ID for " + gameObject.name + " replacing " + id);
            // The following lines tell Unity to let the developer save the change.
            // Undo.RecordObject(this, "New ID for " + gameObject.name);
            id = System.Guid.NewGuid().ToString();
        }

        manager.Register(this);
    }

    void OnDestroy() {
        if (manager != null) manager.Unregister(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Guid : MonoBehaviour {

    public string id;

    private GuidManager manager;

    void Awake() {
        manager = GameObject.FindObjectOfType<GuidManager>();

        if (id == null || id == "" || manager.IsRegisteredAlready(this)) {
            id = System.Guid.NewGuid().ToString();
        }

        manager.Register(this);
    }

    void OnDestroy() {
        manager.Unregister(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject prefab;
    public Transform parent;
    public float existenceCheckInterval;

    private GameObject spawnedObject;

    void Start() {
        if (parent == null) {
            parent = transform;
        }
        InvokeRepeating("CheckExistence", 0, existenceCheckInterval);
    }

    private void CheckExistence() {
        if (!spawnedObject) {
            GameObject spawnedObject = GameObject.Instantiate(prefab, parent);
            spawnedObject.transform.position = transform.position;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour {
    public GameObject[] prefabs;

    void Start() {
        GameObject prefab = Randoms.InList(prefabs);
        Instantiate(prefab, transform);
    }
}

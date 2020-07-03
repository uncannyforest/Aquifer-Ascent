using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour {
    public GameObject prefab;
    public Transform orbParent;
    public float checkInterval = 1f;

    private GameObject orb;
    private ContainerTrigger containerTrigger;

    void Start() {
        if (orbParent == null) {
            orbParent = transform;
        }
        containerTrigger = gameObject.GetComponent<ContainerTrigger>();
        InvokeRepeating("CheckOrb", 0, checkInterval);
    }

    private void CheckOrb() {
        if (orb) {
            return;
        }

        orb = GameObject.Instantiate(prefab, transform.position, transform.rotation, orbParent);
        
        if (containerTrigger != null) {
            containerTrigger.Refresh();
        }
    }
}

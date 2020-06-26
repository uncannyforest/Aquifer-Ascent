using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soluble : MonoBehaviour, State.Stateful {

    public System.Object State { get => state; set => state = (StateFields)value; }

    public StateFields state = new StateFields();
    [Serializable] public class StateFields {
        public bool isDissolved = false;
    }
    public float dissolveTime = 2;
    public LayerMask solventLayerMask;
    public string solventTag = "Solvent";

    private bool isDissolvingCompleted = false;

    void Update() {
        if (state.isDissolved && !isDissolvingCompleted) {
            foreach (Transform child in transform) {
                Vector3 newScale = child.localScale;
                newScale.y -= Time.deltaTime / dissolveTime;

                if (newScale.y <= 0) {
                    newScale.y = 0;
                    isDissolvingCompleted = true;
                }

                child.localScale = newScale;
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & solventLayerMask.value) != 0 && other.CompareTag(solventTag)) {
            other.GetComponent<StandardOrb>().Kill();
            state.isDissolved = true;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soluble : MonoBehaviour
{
    public float dissolveTime = 2;
    public LayerMask solventLayerMask;
    public string solventTag = "Solvent";

    private bool isDissolving = false;

    void Update() {
        if (isDissolving == true) {
            foreach (Transform child in transform) {
                Vector3 newScale = child.localScale;
                newScale.y -= Time.deltaTime / dissolveTime;

                if (newScale.y <= 0) {
                    newScale.y = 0;
                    isDissolving = false;
                }

                child.localScale = newScale;
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & solventLayerMask.value) != 0 && other.CompareTag(solventTag)) {
            other.GetComponent<StandardOrb>().Kill();
            isDissolving = true;
        }
    }

}

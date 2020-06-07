using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriggerExit))]
public class InDarkness : BooleanScript
{
    public LayerMask lightLayerMask;
    public LayerMask wallLayerMask;
    public float checkInterval = 0.2f;
    public bool ignoreNephews;

    public bool IsInDarkness {
        get => inDarkness;
    }
    override public bool IsActive {
        get => inDarkness;
    }

    private List<GameObject> nearbyLights = new List<GameObject>();
    private bool inDarkness = true;

    // Start is called before the first frame update
    void Start()
    {
        if (checkInterval != 0) {
            InvokeRepeating("CheckDarkness", 0.0f, checkInterval);
        }
    }

    void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & lightLayerMask.value) != 0) {
            nearbyLights.Add(other.gameObject);
        }
    }

	void OnTriggerExit(Collider other) {
        if(((1 << other.gameObject.layer) & lightLayerMask.value) != 0) {
            nearbyLights.Remove(other.gameObject);
        }
    }

    public void CheckDarkness() {
        inDarkness = true;
        foreach (GameObject nearbyLight in nearbyLights) {
            if (nearbyLight == null) {
                Debug.LogWarning("Failed to remove dead light :(");
                continue;
            }
            if (nearbyLight.GetComponent<StandardOrb>().spawnState == 0) {
                continue;
            }
            if (ignoreNephews && nearbyLight.transform.parent.parent == this.transform.parent) {
                continue;
            }
            if (!Physics.Linecast(
                    nearbyLight.transform.position,          // putting light first ensures that objects
                    gameObject.transform.transform.position, // outside cave walls are not considered lit
                    out RaycastHit hitInfo,
                    wallLayerMask,
                    QueryTriggerInteraction.Ignore)) {
                inDarkness = false;
                return;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InDarkness : MonoBehaviour
{
    public LayerMask lightLayerMask;
    public LayerMask wallLayerMask;
    public float checkInterval = 0.2f;

    public bool IsInDarkness {
        get => inDarkness;
    }

    private List<GameObject> nearbyLights = new List<GameObject>();
    private bool inDarkness = true;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("CheckDarkness", 0.0f, checkInterval);
    }

    // Update is called once per frame
    void Update()
    {
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

    void CheckDarkness() {
        inDarkness = true;
        foreach (GameObject nearbyLight in nearbyLights) {
            if (!Physics.Linecast(
                    gameObject.transform.transform.position,
                    nearbyLight.transform.position,
                    out RaycastHit hitInfo,
                    wallLayerMask,
                    QueryTriggerInteraction.Ignore)) {
                inDarkness = false;
                return;
            }
        }
        Debug.Log("SCAAAAAAAAAAARY!");
    }
    
}

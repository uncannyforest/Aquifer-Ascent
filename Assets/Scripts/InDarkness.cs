using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriggerExit))]
public class InDarkness : BooleanScript
{
    public LayerMask lightLayerMask;
    public LayerMask wallLayerMask;
    public float checkInterval = 0.2f;

    private Animator animator;

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
        animator = transform.parent.GetComponent<Animator>();

        InvokeRepeating("UpdateDarkness", 0.0f, checkInterval);
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
        Debug.Log("YEP!");
        if(((1 << other.gameObject.layer) & lightLayerMask.value) != 0) {
            nearbyLights.Remove(other.gameObject);
        }
    }

    void CheckDarkness() {
        inDarkness = true;
        foreach (GameObject nearbyLight in nearbyLights) {
            if (nearbyLight == null) {
                Debug.Log("Failed to remove dead light :(");
                continue;
            }
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
    }
    
    void UpdateDarkness() {
        bool oldInDarkness = inDarkness;

        CheckDarkness();

        if (oldInDarkness ^ inDarkness) {
            animator.SetBool("InDarkness", inDarkness);
        }
    }

}

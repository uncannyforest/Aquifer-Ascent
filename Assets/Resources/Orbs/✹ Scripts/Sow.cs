using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
[RequireComponent(typeof(StandardOrb))]
public class Sow : MonoBehaviour
{
    public GameObject treePrefab;
    public float sowTime = 15;
    public string actionName = "sow";

    private Transform fertileGround;
    private bool sown = false;
    private bool hitGround = false;
    Vector3 groundPosition;
    Vector3 destinationPosition;
    private float sowState = 0;

    Collider physicsCollider;
    Rigidbody myRigidbody;
    StandardOrb standardOrbScript;
    Holdable holdableScript;

    void Start() {
        physicsCollider = GetComponent<Collider>();
        myRigidbody = GetComponent<Rigidbody>();
        standardOrbScript = GetComponent<StandardOrb>();
        holdableScript = GetComponent<Holdable>();
    }

    void Update() {
        if (hitGround) {
            if (sowState >= 1) {
                InitiateTree();
                return;
            }

            sowState += Time.deltaTime / sowTime;
            transform.position = Vector3.Lerp(groundPosition, destinationPosition, sowState);
            standardOrbScript.SetOrbIntensity(1 - sowState);
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "CanSowHere" && other.GetComponentInChildren<OrbTree>() == null) {
            fertileGround = other.transform;
            holdableScript.SetOptionalAction(actionName);
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.transform == fertileGround && !sown) {
            fertileGround = null;
            holdableScript.SetOptionalAction(null);
		}
    }

    void Use() {  
        sown = true;
        transform.parent.parent.GetComponent<HoldObject>().OnDropObject(gameObject, true);
    }

    void UpdateHeldState(float heldState) {
        if (sown && heldState == 0) {
            hitGround = true;
            groundPosition = transform.position;
            destinationPosition = groundPosition + Vector3.down * holdableScript.GetColliderWidth() / 2;
        }  
    }

    void InitiateTree() {
        GameObject tree = Instantiate(treePrefab, fertileGround, true);
        tree.transform.position = groundPosition;

        GameObject.Destroy(this.gameObject);
    }
}

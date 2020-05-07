using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
[RequireComponent(typeof(StandardOrb))]
public class Sow : MonoBehaviour
{
    public float sowTime = 15;

    private bool sown = false;
    private bool hitGround = false;
    Vector3 groundPosition;
    Vector3 destinationPosition;
    private float sowState = 0;

    Collider physicsCollider;
    Rigidbody myRigidbody;
    StandardOrb standardOrbScript;

    void Start() {
        physicsCollider = GetComponent<Collider>();
        myRigidbody = GetComponent<Rigidbody>();
        standardOrbScript = GetComponent<StandardOrb>();
    }

    void Update() {
        if (hitGround) {
            sowState += Time.deltaTime / sowTime;
            transform.position = Vector3.Lerp(groundPosition, destinationPosition, sowState);
            standardOrbScript.SetOrbIntensity(1 - sowState);
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
            destinationPosition = groundPosition + Vector3.down * GetComponent<Holdable>().GetColliderWidth() / 2;
        }  
    }
}

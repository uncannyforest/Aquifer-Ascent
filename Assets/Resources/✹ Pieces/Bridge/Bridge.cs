using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(Holdable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Bridge : MonoBehaviour {
    public float holdAngle = 60f;
    public float dropDisplacement = .5f;
    public float dropTorque = 1f;
    public float maxStationarySpeed = .01f;
    public float newWidth = .75f;
    public float newDepth = .125f;
    public float mountYTolerance = .25f;
    public float unmountTopDistance = 0f;
    public float unmountBottomDistance = .1f;
    public bool placed = false;

    private Rigidbody myRigidbody;
    private BoxCollider myCollider;
    private float height;
    public float depth;
    private Transform model;
    private Transform player;

    void Start() {
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();
        height = myCollider.bounds.size.y - unmountTopDistance;
        player = GameObject.FindObjectOfType<ThirdPersonCharacter>().transform;
        depth = myCollider.bounds.max.z - player.GetComponent<CapsuleCollider>().bounds.min.y;
        model = transform.GetChild(2);
    }

    void UpdateHeldState(float heldState) {
        transform.rotation = Quaternion.Lerp(player.rotation * Quaternion.Euler(30, 0, 0), player.rotation * Quaternion.Euler(0, 0, holdAngle), heldState);
        model.localPosition = Vector3.down * height / 2 * heldState;

        if (heldState == 0f) StartCoroutine(Place());
        else {
            StopAllCoroutines();
            placed = false;
            myCollider.enabled = false;
            myRigidbody.isKinematic = true;
        }
    }

    private IEnumerator Place() {
        Debug.Log("Placing!");
        myCollider.enabled = true;
        myRigidbody.isKinematic = false;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        while (myRigidbody.velocity.magnitude > maxStationarySpeed) {
            yield return new WaitForFixedUpdate();
        }
        myRigidbody.isKinematic = true;

        Vector3 eulers = myRigidbody.rotation.eulerAngles;
        if (eulers.x > 180) eulers.x -= 360;
        if (eulers.z > 180) eulers.z -= 360;
        float sizeY = myCollider.size.y;
        Vector3 newBounds;
        if (Mathf.Abs(eulers.x) < Mathf.Abs(eulers.z)) {
            newBounds = new Vector3(newDepth, sizeY, newWidth);
        } else {
            newBounds = new Vector3(newWidth, sizeY, newDepth);
        }
        myCollider.size = newBounds;

        placed = true;
    }
}
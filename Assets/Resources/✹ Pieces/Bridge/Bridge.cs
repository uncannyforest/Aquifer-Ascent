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
    public Vector3 dropRotation1 = new Vector3(30, 0, 0);
    public Vector3 dropRotation2 = new Vector3(75, 0, 0);
    public float maxStationarySpeed = .01f;
    public float newWidth = .75f;
    public float newDepth = .125f;
    public float mountYTolerance = .25f;
    public float unmountTopDistance = 0f;
    public float unmountBottomDistance = .1f;
    public bool placed = false;

    private int placingMode = 0;

    private Rigidbody myRigidbody;
    private BoxCollider myCollider;
    public float height;
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
        Quaternion dropRotation = placingMode == 1 ? Quaternion.Euler(dropRotation1)
            : Quaternion.Euler(dropRotation2);
        Debug.Log("placingMode" + placingMode);
        if (placingMode != 3)
            transform.rotation = Quaternion.Lerp(player.rotation * dropRotation, player.rotation * Quaternion.Euler(0, 0, holdAngle), heldState);
        if (placingMode != 3)
            model.localPosition = Vector3.down * height / 2 * heldState;

        if (heldState == 0f) StartCoroutine(Place());
        else {
            StopAllCoroutines();
            placed = false;
            myCollider.enabled = false;
            myRigidbody.isKinematic = true;
            if (placingMode == -1) {
                float worldHeight = height * transform.localScale.y;
                if (!Physics.Raycast(transform.position, player.TransformDirection(Quaternion.Euler(dropRotation1) * Vector3.up), worldHeight, LayerMask.NameToLayer("Player"))) {
                    Debug.Log("Placing position 1");
                    Debug.DrawLine(transform.position, transform.position + player.TransformDirection(Quaternion.Euler(dropRotation1) * Vector3.up * worldHeight), Color.green, 600);
                    placingMode = 1;
                } else if (!Physics.Raycast(transform.position, player.TransformDirection(Quaternion.Euler(dropRotation2) * Vector3.up), worldHeight, LayerMask.NameToLayer("Player"))) {
                    Debug.Log("Placing position 2");
                    Debug.DrawLine(transform.position, transform.position + player.TransformDirection(Quaternion.Euler(dropRotation2) * Vector3.up * worldHeight), Color.green, 600);
                    placingMode = 2;
                } else {
                    Debug.Log("Placing position 3");
                    Debug.DrawLine(transform.position, transform.position + player.TransformDirection(Quaternion.Euler(dropRotation1) * Vector3.up * worldHeight), Color.green, 600);
                    Debug.DrawLine(transform.position, transform.position + player.TransformDirection(Quaternion.Euler(dropRotation2) * Vector3.up * worldHeight), Color.green, 600);
                    placingMode = 3;
                }
            }
            if (heldState == 1f) {
                placingMode = -1;
            }
        }
    }

    private IEnumerator Place() {
        if (placingMode == 3) {
            transform.position = model.position;
            model.localPosition = Vector3.zero;
        }
        placingMode = 0;
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
            newBounds = new Vector3(newDepth, sizeY, newWidth * transform.localScale.x);
        } else {
            newBounds = new Vector3(newWidth, sizeY, newDepth);
        }
        myCollider.size = newBounds;

        placed = true;
    }
}
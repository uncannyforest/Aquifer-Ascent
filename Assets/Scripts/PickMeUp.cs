using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PickMeUp : MonoBehaviour
{

    public AudioClip pickUpSound;
    public AudioClip setDownSound;
    public float pickUpTime = 0.5f;

    private float heldState = 0.0f; // 0 if not held, 1 if held
    private Transform originalParent;
    private Transform playerHoldTransform;
    Collider physicsCollider;
    Rigidbody myRigidbody;
    bool isMoving = false;
    Vector3 oldPosition;
    private AudioSource objectAudio; 
    private Bounds myColliderBounds;

    public bool PickedUp {
        get => this.transform.parent == playerHoldTransform;
    }

    void Start(){
        originalParent = this.transform.parent.transform;
        playerHoldTransform = GameObject.FindWithTag("Player").transform.Find("HoldLocation");
        physicsCollider = GetComponent<Collider>();
        myColliderBounds = physicsCollider.bounds;
        myRigidbody = GetComponent<Rigidbody>();
        objectAudio = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update(){

        if (isMoving) {
            heldState += Time.deltaTime / pickUpTime;

            Vector3 newPosition = playerHoldTransform.position;

            if (heldState >= 1f) {
                EndPickUp();
            } else {
                gameObject.SendMessage("UpdateHeldState", heldState);
                this.transform.position =
                        Vector3.Lerp(oldPosition, newPosition, QuadInterpolate(heldState));
            }
        }
    }

    public void SetDown(){
        objectAudio.PlayOneShot(setDownSound, 0.5f);
        physicsCollider.enabled = true;
        this.transform.SetParent(originalParent);
        myRigidbody.isKinematic = false;
        heldState = 0;
        gameObject.SendMessage("UpdateHeldState", heldState);
    }
    public void StartPickUp(){
        objectAudio.PlayOneShot(pickUpSound, 0.5f);
        physicsCollider.enabled = false;
        myRigidbody.isKinematic = true;
        isMoving = true;
        oldPosition = this.transform.position;
        this.transform.rotation = playerHoldTransform.rotation;
        this.transform.SetParent(playerHoldTransform);
    }

    private void EndPickUp() {
        isMoving = false;
        this.transform.position = playerHoldTransform.position;
        heldState = 1;
        gameObject.SendMessage("UpdateHeldState", heldState);

    }

    public float GetColliderWidth(){
        // this is broken out here becuase bounds can only be queried when collider is active
        return myColliderBounds.size.z;
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }
}


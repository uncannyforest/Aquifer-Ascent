using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: split into OrbPickUp and PickMeUp once we have other grabbables
[RequireComponent(typeof(Collider))]
public class PickMeUp : MonoBehaviour
{

    public float pickUpTime = 0.5f;
    public float heldIntensity = 0.5f;

    private Transform originalParent;
    private Transform playerHoldTransform;
    Collider physicsCollider;
    Rigidbody myRigidbody;
    private StandardOrb orbScript;
    bool isMoving = false;
    float moveProgress;
    Vector3 oldPosition;
    private AudioSource objectAudio; 
    public AudioClip pickUpSound;
    public AudioClip setDownSound;
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
        orbScript = GetComponent<StandardOrb>();
    }


    // Update is called once per frame
    void Update(){

        if (isMoving) {
            moveProgress += Time.deltaTime / pickUpTime;

            Vector3 newPosition = playerHoldTransform.position;

            if (moveProgress >= 1f) {
                EndPickUp();
            } else {
                orbScript.setOrbIntensity(1 - (1 - heldIntensity) * moveProgress);
                this.transform.position =
                        Vector3.Lerp(oldPosition, newPosition, QuadInterpolate(moveProgress));
            }
        }
    }

    public void SetDown(){
        objectAudio.PlayOneShot(setDownSound, 0.5f);
        physicsCollider.enabled = true;
        this.transform.SetParent(originalParent);
        myRigidbody.isKinematic = false;
        orbScript.setOrbIntensity(1);
    }
    public void StartPickUp(){
        objectAudio.PlayOneShot(pickUpSound, 0.5f);
        physicsCollider.enabled = false;
        myRigidbody.isKinematic = true;
        isMoving = true;
        moveProgress = 0f;
        oldPosition = this.transform.position;
        this.transform.rotation = playerHoldTransform.rotation;
        this.transform.SetParent(playerHoldTransform);
    }

    private void EndPickUp() {
        isMoving = false;
        this.transform.position = playerHoldTransform.position;
        orbScript.setOrbIntensity(heldIntensity);

    }

    public float GetColliderWidth(){
        // this is broken out here becuase bounds can only be queried when collider is active
        return myColliderBounds.size.z;
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }
}


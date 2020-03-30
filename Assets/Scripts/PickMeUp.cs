using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickMeUp : MonoBehaviour
{

    public float pickUpTime = 0.5f;

    private Rigidbody rb;
    private Transform playerHoldTransform;
    Collider physicsCollider;
    Rigidbody myRigidbody;
    bool isMoving = false;
    float moveProgress;
    Vector3 oldPosition;
    private AudioSource objectAudio; 
    public AudioClip pickUpSound;
    public AudioClip setDownSound;

    public bool PickedUp {
        get => this.transform.parent == playerHoldTransform;
    }

    void Awake(){
        playerHoldTransform = GameObject.FindWithTag("Player").transform.Find("HoldLocation");
        //Fetch the GameObject's Collider (make sure it has a Collider component)
        physicsCollider = this.transform.Find("Sphere").GetComponent<Collider>();
        myRigidbody = GetComponent<Rigidbody>();
        objectAudio = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update(){

        if (isMoving) {
            moveProgress += Time.deltaTime / pickUpTime;

            Vector3 newPosition = playerHoldTransform.position;

            if (moveProgress >= 1f) {
                EndPickUp();
            } else {
                this.transform.position =
                        Vector3.Lerp(oldPosition, newPosition, QuadInterpolate(moveProgress));
            }
        }
    }

    public void SetDown(){
        objectAudio.PlayOneShot(setDownSound, 0.5f);
        physicsCollider.enabled = true;
        myRigidbody.isKinematic = false;
        this.transform.SetParent(null);
    }
    public void StartPickUp(){
        objectAudio.PlayOneShot(pickUpSound, 0.5f);
        physicsCollider.enabled = false;
        myRigidbody.isKinematic = true;
        isMoving = true;
        moveProgress = 0f;
        oldPosition = this.transform.position;
        this.transform.SetParent(playerHoldTransform);
    }

    private void EndPickUp() {
        isMoving = false;
        this.transform.position = playerHoldTransform.position;
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }
}


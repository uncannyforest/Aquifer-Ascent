using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickMeUp : MonoBehaviour
{

    public float pickUpTime = 0.5f;

    private Rigidbody rb;
    private Transform playerHoldTransform;
    Collider myCollider;
    Rigidbody myRigidbody;
    public bool pickedUp = false;
    bool isMoving = false;
    float moveProgress;
    Vector3 oldPosition;
    public PlayerInputActions actions;
    bool playerIsNearEnough = false;
    private AudioSource objectAudio; 
    public AudioClip pickUpSound;
    public AudioClip setDownSound;


    void Awake(){
        actions = new PlayerInputActions();

        // delagate or event -type syntax, which is kinda weird looking ... 
        // when the "PickUp" type Input is performed, call Interact()
        actions.PlayerControls.PickUp.performed += _ => Interact();

        playerHoldTransform = GameObject.FindWithTag("Player").transform.Find("HoldLocation");
        //Fetch the GameObject's Collider (make sure it has a Collider component)
        myCollider = GetComponent<Collider>();
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

    private void OnEnable(){
        actions.Enable();
    }

    private void OnDisable(){
        actions.Disable();
    }


    void OnTriggerEnter(Collider other) {
        if(other.tag == "Player"){
            playerIsNearEnough = true;
                        Debug.Log("Near = true");

        }
    }

	void OnTriggerExit(Collider other) {
		if(other.tag == "Player"){
			playerIsNearEnough = false;
                        Debug.Log("Too far away from this object!");

		}
    }
    
    void Interact(){
        //Debug.Log("Doing a thing!!!! With new input system woo");

        if(!playerIsNearEnough){
            return;
        }
        // if you're not already holding it
        // and you're not holding something else
        if(!pickedUp & playerHoldTransform.childCount == 0){
            // Debug.Log("Picking up object yay!");
            StartPickUp();
        }

        else if(pickedUp){
            // Debug.Log("Set thing down byeeee");
            SetDown();
        }
    }

    private void SetDown(){
        objectAudio.PlayOneShot(setDownSound, 0.5f);
        this.transform.SetParent(null);
        pickedUp = false;
    }
    private void StartPickUp(){
        objectAudio.PlayOneShot(pickUpSound, 0.5f);
        myCollider.enabled = false;
        myRigidbody.isKinematic = true;
        pickedUp = true;
        isMoving = true;
        moveProgress = 0f;
        oldPosition = this.transform.position;
    }

    private void EndPickUp() {
        isMoving = false;
        this.transform.position = playerHoldTransform.position;
        this.transform.SetParent(playerHoldTransform);
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }

}

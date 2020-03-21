using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickMeUp : MonoBehaviour
{

    public float pickUpTime = 0.5f;

    private Rigidbody rb;
    private GameObject player;
    Collider m_Collider;
    Rigidbody m_Rigidbody;
    public bool pickedUp = false;
    bool isMoving = false;
    float moveProgress;
    Vector3 oldPosition;

    void Start(){
        player = GameObject.FindWithTag("Player");
        //Fetch the GameObject's Collider (make sure it has a Collider component)
        m_Collider = GetComponent<Collider>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    void Update(){
    /*  if (Input.GetButton("pickup"))
        { // Define it in the input manager
            // do something...
        }
    */
        if (isMoving) {
            moveProgress += Time.deltaTime / pickUpTime;

            Vector3 newPosition = player.transform.Find("HoldLocation").transform.position;

            if (moveProgress >= 1f) {
                EndPickUp();
            } else {
                this.transform.position =
                        Vector3.Lerp(oldPosition, newPosition, QuadInterpolate(moveProgress));
            }
        }
    }

    void OnMouseDown(){
        if (!pickedUp){
            StartPickUp();
        }
    }


    private void StartPickUp(){
        // Sets "newParent" as the new parent of the child GameObject.
        Debug.Log("You clicked me!");
        m_Collider.enabled = false;
        m_Rigidbody.isKinematic = true;
        pickedUp = true;
        isMoving = true;
        moveProgress = 0f;
        oldPosition = this.transform.position;
    }

    private void EndPickUp() {
        isMoving = false;
        this.transform.position = player.transform.Find("HoldLocation").transform.position;
        this.transform.SetParent(player.transform);
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickMeUp : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject player;
    Collider m_Collider;
    public bool pickedUp = false;

    void Start(){
        player = GameObject.FindWithTag("Player");
        //Fetch the GameObject's Collider (make sure it has a Collider component)
        m_Collider = GetComponent<Collider>();
    }


    // Update is called once per frame
    void Update(){
    /*  if (Input.GetButton("pickup"))
        { // Define it in the input manager
            // do something...
        }
    */
    }

    void OnMouseDown(){
        if (!pickedUp){
            PickUp();
        }
    }


    private void PickUp(){
        // Sets "newParent" as the new parent of the child GameObject.
        Debug.Log("You clicked me!");
        this.transform.SetParent(player.transform);
        this.transform.position = player.transform.Find("HoldLocation").transform.position;
        m_Collider.enabled = false;
        pickedUp = true;
    }


}

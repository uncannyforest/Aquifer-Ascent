using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    public PlayerInputActions actions;
    private Transform playerHoldTransform;
    private HashSet<GameObject> nearObjects = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        actions = new PlayerInputActions();

        // delagate or event -type syntax, which is kinda weird looking ... 
        // when the "PickUp" type Input is performed, call Interact()
        actions.PlayerControls.PickUp.performed += _ => Interact();

        playerHoldTransform = gameObject.transform.Find("HoldLocation");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable(){
        actions.Enable();
    }

    private void OnDisable(){
        actions.Disable();
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("Hiiiii");
        if(other.tag == "CanPickUp") {
            GameObject objectToPickUp;
            if(other.gameObject.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.gameObject;
            } else if (other.transform.parent.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.transform.parent.gameObject;
            } else {
                Debug.LogError("Object tagged CanPickUp has no PickMeUp script on it or parent");
                return;
            }
            nearObjects.Add(objectToPickUp);
            Debug.Log("Near = true");
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.tag == "CanPickUp") {
            GameObject objectToPickUp;
            if(other.gameObject.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.gameObject;
            } else if (other.transform.parent.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.transform.parent.gameObject;
            } else {
                Debug.LogError("Object tagged CanPickUp has no PickMeUp script on it or parent");
                return;
            }
			bool foundObject = nearObjects.Remove(objectToPickUp);
            if (!foundObject) {
                Debug.LogWarning("Tried to remove object from nearObjects that was not there");
            }
            Debug.Log("Too far away from this object!");
		}
    }
    
    void Interact() {
        if (playerHoldTransform.childCount > 0) {
            DropAnyPickedUpObjects();
            return;
        }

        if (nearObjects.Count == 0) {
            return;
        }

        GameObject closestObject = nearObjects.OrderBy(
                o => Vector3.Distance(o.transform.position, gameObject.transform.position)
            ).First();

        closestObject.GetComponent<PickMeUp>().StartPickUp();
    }

    void DropAnyPickedUpObjects() {
        foreach (Transform child in playerHoldTransform) {
            PickMeUp childPickMeUp = child.GetComponent<PickMeUp>();
            if (childPickMeUp == null) {
                Debug.LogWarning("Child of playerHold had no PickMeUp script!");
            } else {
                childPickMeUp.SetDown();
            }
        }
    }

}

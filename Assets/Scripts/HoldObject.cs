using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HoldObject : MonoBehaviour
{
    public float transitionTime = .5f;
    public float handWeightCurrent = 0;

    private Transform playerHoldTransform;
    private HashSet<GameObject> nearObjects = new HashSet<GameObject>();
    Animator m_Animator;
    private float heldObjectWidth;

    FlexibleInput flexibleInput;
    EnvironmentInteractor environment;

    public bool IsHolding {
        get => this.playerHoldTransform.childCount > 0;
    }

    // Start is called before the first frame update
    void Awake()
    {
        flexibleInput = new FlexibleInput(this);
        environment = new EnvironmentInteractor(this);

        playerHoldTransform = gameObject.transform.Find("HoldLocation");
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SimpleInput.GetButtonDown("Interact1")) {
            Interact();
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "CanPickUp") {
            GameObject interactableObject = environment.GetInteractableObject(other.gameObject);
            nearObjects.Add(interactableObject);
            flexibleInput.UpdateDisplayForNearbyObjects(nearObjects);
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.tag == "CanPickUp") {
            GameObject interactableObject = environment.GetInteractableObject(other.gameObject);
			bool foundObject = nearObjects.Remove(interactableObject);
            if (!foundObject) {
                Debug.LogWarning("Tried to remove object from nearObjects that was not there");
            }
            flexibleInput.UpdateDisplayForNearbyObjects(nearObjects);
		}
    }
    
    void Interact() {
        if (IsHolding) {
            environment.DropHeldObject(playerHoldTransform);
            flexibleInput.UpdateDisplayForNearbyObjects(nearObjects);
            return;
        }

        if (nearObjects.Count == 0) {
            return;
        }

        GameObject heldObject = environment.HoldClosestObject(nearObjects);
        heldObjectWidth = heldObject.GetComponent<Holdable>().GetColliderWidth();
        flexibleInput.UpdateDisplayForHeldObject(heldObject);
    }

    void OnAnimatorIK()
    {
        if(playerHoldTransform.childCount < 1 && handWeightCurrent == 0){
            return;
        }

        Vector3 rightHandlePosition = playerHoldTransform.position + (.5f * heldObjectWidth * this.transform.right);
        Vector3 leftHandlePosition = playerHoldTransform.position - (.5f * heldObjectWidth * this.transform.right);

        if(playerHoldTransform.childCount > 0) { // if you're holding something 
            if( handWeightCurrent < 1f ){
                handWeightCurrent += Time.deltaTime / transitionTime;
            }else{
                handWeightCurrent = 1f;
            }
        }else{
            // Let the hands relax :)

            if( handWeightCurrent > 0f ){
                handWeightCurrent -= Time.deltaTime / transitionTime;
            }else{
                handWeightCurrent = 0f;
            }
        }

        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand,handWeightCurrent);
        m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand,handWeightCurrent); 	
        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,handWeightCurrent);
        m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,handWeightCurrent); 

        m_Animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandlePosition);
        m_Animator.SetIKRotation(AvatarIKGoal.RightHand,playerHoldTransform.rotation);
        m_Animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandlePosition);
        m_Animator.SetIKRotation(AvatarIKGoal.LeftHand,playerHoldTransform.rotation);  

    }
    
}

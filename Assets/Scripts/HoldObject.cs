using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HoldObject : MonoBehaviour
{
    public float transitionTime = .5f;
    public float handWeightCurrent = 0;

    private Transform playerHoldTransform;
    Animator m_Animator;
    private float heldObjectWidth;

    FlexibleInput flexibleInput;
    EnvironmentInteractor environmentInteractor;

    public bool IsHolding {
        get => this.playerHoldTransform.childCount > 0;
    }

    // Start is called before the first frame update
    void Awake()
    {
        flexibleInput = new FlexibleInput(this);
        environmentInteractor = new EnvironmentInteractor(this);

        playerHoldTransform = gameObject.transform.Find("HoldLocation");
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SimpleInput.GetButtonDown("Interact1")) {
            Interact1();
        }
        if (SimpleInput.GetButtonDown("Interact2")) {
            Interact2();
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "CanPickUp") {
            environmentInteractor.AddInteractableObject(other.gameObject);
            flexibleInput.UpdateDisplayForNearbyObjects(environmentInteractor.NearObjects);
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.tag == "CanPickUp") {
            environmentInteractor.RemoveInteractableObject(other.gameObject);
            flexibleInput.UpdateDisplayForNearbyObjects(environmentInteractor.NearObjects);
		}
    }

    /// <summary> Called by Holdable script once hold initiated </summary>
    public void OnHoldObject(GameObject heldObject) {
        heldObjectWidth = heldObject.GetComponent<Holdable>().GetColliderWidth();
        flexibleInput.UpdateDisplayForHeldObject(heldObject);
    }

    /// <summary> Called by Holdable script since sometimes child initiates SetDown </summary>
    public void OnDropObject(GameObject heldObject) {
        flexibleInput.UpdateDisplayForNearbyObjects(environmentInteractor.NearObjects);
    }

    void Interact1() {
        if (IsHolding) {
            environmentInteractor.DropHeldObject(playerHoldTransform);
        } else {
            environmentInteractor.HoldClosestObject();
        }
    }

    void Interact2() {
        if (IsHolding) {
            environmentInteractor.UseHeldObject(playerHoldTransform);
        }
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

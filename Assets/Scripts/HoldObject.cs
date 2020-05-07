using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HoldObject : MonoBehaviour
{
    public float transitionTime = .5f;
    public float handWeightCurrent = 0;
    public Transition isTransitioning = Transition.None;

    public enum Transition { None, Holding, Dropping };

    private Transform playerHoldTransform;
    Animator m_Animator;
    private float heldObjectWidth;

    FlexibleInputDisplay inputDisplay;
    EnvironmentInteractor environmentInteractor;

    public bool IsHolding {
        get => this.playerHoldTransform.childCount > 0;
    }

    // Start is called before the first frame update
    void Awake()
    {
        inputDisplay = new FlexibleInputDisplay(this);
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
            inputDisplay.UpdateForNearbyObjects(environmentInteractor.NearObjects);
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.tag == "CanPickUp") {
            environmentInteractor.RemoveInteractableObject(other.gameObject);
            inputDisplay.UpdateForNearbyObjects(environmentInteractor.NearObjects);
		}
    }

    /// <summary> Called by Holdable script once hold initiated </summary>
    public void OnHoldObject(GameObject heldObject) {
        heldObjectWidth = heldObject.GetComponent<Holdable>().GetColliderWidth();
        inputDisplay.UpdateForHeldObject(heldObject);
    }

    /// <summary> Called by Holdable script since sometimes child initiates SetDown </summary>
    public void OnDropObject(GameObject heldObject, bool stoop) {
        if (stoop) {
            m_Animator.SetTrigger("Stoop");
            isTransitioning = Transition.Dropping;
            inputDisplay.UpdateNoActions();
        } else {
            inputDisplay.UpdateForNearbyObjects(environmentInteractor.NearObjects);
        }
    }

    void OnMidStoop() {
        if (isTransitioning == Transition.Dropping) {
            environmentInteractor.NotifyHeldObjectReadyToDrop(playerHoldTransform);
        }
    }

    void OnFinishStoop() {
        if (isTransitioning == Transition.Dropping) {
            inputDisplay.UpdateForNearbyObjects(environmentInteractor.NearObjects);
            isTransitioning = Transition.None;
        }
    }

    void Interact1() {
        if (isTransitioning != Transition.None) {
            return;
        }
        if (IsHolding) {
            environmentInteractor.DropHeldObject(playerHoldTransform);
        } else {
            environmentInteractor.HoldClosestObject();
        }
    }

    void Interact2() {
        if (isTransitioning != Transition.None) {
            return;
        }
        if (IsHolding) {
            environmentInteractor.UseHeldObject(playerHoldTransform);
        }
    }

    void OnAnimatorIK()
    {
        if(!IsHolding && handWeightCurrent == 0){
            return;
        }

        Vector3 rightHandlePosition = playerHoldTransform.position + (.5f * heldObjectWidth * this.transform.right);
        Vector3 leftHandlePosition = playerHoldTransform.position - (.5f * heldObjectWidth * this.transform.right);

        if(IsHolding) { // if you're holding something 
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TriggerExit))]
public class HoldObject : MonoBehaviour
{
    public float transitionTime = 1f;
    public Transition isTransitioning = Transition.None;

    public enum Transition { None, Holding, Dropping };

    private Transform playerHoldTransform;

    FlexibleInputDisplay inputDisplay;
    EnvironmentInteractor environmentInteractor;
    HoldAnimationControl holdAnimationControl;

    public bool IsHolding {
        get => this.playerHoldTransform.childCount > 0;
    }

    // Start is called before the first frame update
    void Awake() {
        playerHoldTransform = gameObject.transform.Find("HoldLocation");

        inputDisplay = new FlexibleInputDisplay(this);
        environmentInteractor = new EnvironmentInteractor(this, playerHoldTransform);
        holdAnimationControl = new HoldAnimationControl(this, playerHoldTransform);
    }

    // Update is called once per frame
    void Update() {
        if (SimpleInput.GetButtonDown("Interact1")) {
            Interact1();
        }
        if (SimpleInput.GetButtonDown("Interact2")) {
            Interact2();
        }
        holdAnimationControl.UpdatePlayerHoldTransform();
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
        holdAnimationControl.heldObjectWidth = heldObject.GetComponent<Holdable>().GetColliderWidth();
        inputDisplay.UpdateForHeldObject(heldObject);
    }

    /// <summary> Called by Holdable script since sometimes child initiates SetDown </summary>
    public void OnDropObject(GameObject heldObject, bool stoop) {
        if (stoop) {
            isTransitioning = Transition.Dropping;
            inputDisplay.UpdateNoActions();
            Transform groundTransform = holdAnimationControl.StartStoop();
            environmentInteractor.NotifyGroundObject(groundTransform);
        } else {
            inputDisplay.UpdateForNearbyObjects(environmentInteractor.NearObjects);
        }
    }

    void OnMidStoop() {
        if (isTransitioning == Transition.Dropping) {
            environmentInteractor.NotifyHeldObjectReadyToDrop();
        }
    }

    void OnFinishStoop() {
        if (isTransitioning == Transition.Dropping) {
            inputDisplay.UpdateForNearbyObjects(environmentInteractor.NearObjects);
            isTransitioning = Transition.None;
            holdAnimationControl.ResetPlayerHoldTransform();
        }
    }

    void Interact1() {
        if (isTransitioning != Transition.None) {
            return;
        }
        if (IsHolding) {
            environmentInteractor.DropHeldObject();
        } else {
            environmentInteractor.HoldClosestObject();
        }
    }

    void Interact2() {
        if (isTransitioning != Transition.None) {
            return;
        }
        if (IsHolding) {
            environmentInteractor.UseHeldObject();
        }
    }

    void OnAnimatorIK() {
        holdAnimationControl.AnimatorIK();
    }
    
}

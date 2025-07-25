﻿using System;
using System.Linq;
using UnityEngine;

public class HoldObject : MonoBehaviour
{
    public Transform playerHoldTransform;
    public float transitionTime = 1f;
    public Transition isTransitioning = Transition.None;

    public enum Transition { None, Holding, Dropping };

    public Action<GameObject> Hold;

    public FlexibleInputDisplay inputDisplay;
    public EnvironmentInteractor environmentInteractor;
    HoldAnimationControl holdAnimationControl;

    public bool IsHolding {
        get => this.playerHoldTransform.childCount > 0;
    }

    // Start is called before the first frame update
    void Start() {
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
        playerHoldTransform.localPosition = heldObject.GetComponent<Holdable>().relativePosition;
        inputDisplay.UpdateForHeldObject(heldObject);
        if (Hold != null) Hold(heldObject);
    }

    /// <summary> Called by Holdable script since sometimes child initiates SetDown </summary>
    public void OnDropObject(GameObject heldObject, bool stoop) {
        if (stoop) {
            isTransitioning = Transition.Dropping;
            inputDisplay.UpdateNoActions();
            Transform groundTransform = holdAnimationControl.StartStoop();
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

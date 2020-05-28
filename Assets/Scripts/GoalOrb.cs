using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
[RequireComponent(typeof(StandardOrb))]
[RequireComponent(typeof(FloatWanderAI))]
public class GoalOrb : MonoBehaviour
{
    public GameObject goalArea;
    public Color successColor = new Color(.75f, .875f, 1f);

    private bool currentlyHeld = false;
    private bool withinGoal = false;
    private Vector3 successStartingPosition = Vector3.negativeInfinity;
    private Color successStartingColor = Color.black; 

    private LineRenderer hintLine;
    private StandardOrb orbScript;
    private FloatWanderAI wanderAI;

    void Start() {
        this.hintLine = transform.Find("Hint Flare").GetComponent<LineRenderer>();
        this.hintLine.enabled = false;
        this.orbScript = GetComponent<StandardOrb>();
        this.wanderAI = GetComponent<FloatWanderAI>();
    }

    void Update() {
        if (hintLine.enabled) {
            UpdateHintLine();
        }
    }

    void OnTriggerEnter(Collider other) {
        if (goalArea.GetComponent<Collider>() == other) {
            hintLine.enabled = false;

            withinGoal = true;
        }
    }

	void OnTriggerExit(Collider other) {
        if (goalArea.GetComponent<Collider>() == other) {
            hintLine.enabled = true;
            UpdateHintLine();

            withinGoal = false;
        }
    }

    void UpdateHeldState(float heldState) {
        if (heldState == 1 && !currentlyHeld) {
            hintLine.enabled = true;
            UpdateHintLine();

            currentlyHeld = true;
        } else if (heldState < 1 && currentlyHeld) {
            hintLine.enabled = false;
            currentlyHeld = false;
            if (withinGoal) {
                Succeed();
            }
        }
        if (!Vector3.negativeInfinity.Equals(successStartingPosition)) {
            transform.position = Vector3.Lerp(goalArea.transform.position, successStartingPosition, CubicInterpolate(heldState));
            orbScript.SetOrbColor(Color.Lerp(successColor, successStartingColor, heldState));
        }
    }

    private void UpdateHintLine() {
        hintLine.SetPosition(1, transform.InverseTransformPoint(goalArea.transform.position));
    }

    private void Succeed() {
        successStartingPosition = transform.position;
        successStartingColor = orbScript.GetColorFromCharge();

        orbScript.currentChargeLevel = 1.0f; // disables recolor
        wanderAI.CanMove = false;

        TriggerObjectDestroyer.Hide(gameObject); // must happen before untagging :P
        transform.Find("Halo").tag = "Untagged"; // remove CanPickUp
    }

    private float CubicInterpolate(float x) {
        return 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ApproachingDarkness))]
public class DarknessRescue : MonoBehaviour {
    public ThirdPersonCharacter character;
    public InDarkness darknessCheck;
    public float stuckDelayFixTime = 3f;
    public float unstuckForce = 1f;

    public Action TimeElapsedStuck;

    private float struggleTime;

    private Transform darknessStruggleChecks;
    private Rigidbody myRigidbody;
    private ApproachingDarkness approachingDarkness;

    private Vector3 rescueDirection = Vector3.zero;
    private bool isStuck = false;

    /// Call only on state transition. Assumes previous state was opposite.
    public bool IsStuck {
        get => isStuck;
        set {
            isStuck = value;
            if (value) {
                Debug.Log("Stuck!");
                struggleTime = Time.time + stuckDelayFixTime;
                Invoke("Rescue", stuckDelayFixTime);
            } else {
                Debug.Log("Not stuck!");
                CancelInvoke();
                rescueDirection = Vector3.zero;
                struggleTime = 0;
            }
        }
    }

    void Start() {
        darknessStruggleChecks = transform.Find("DarknessStruggle");
        myRigidbody = character.GetComponent<Rigidbody>();
        approachingDarkness = GetComponent<ApproachingDarkness>();
    }

    void FixedUpdate() {
        if (rescueDirection != Vector3.zero) {
            Vector3 move = Vector3.ProjectOnPlane(rescueDirection, character.groundNormal);
            myRigidbody.AddForce(move);
        }
    }

    private void Rescue() {
        if (struggleTime != 0 && Time.time >= stuckDelayFixTime) {
            Vector3 positionSum = Vector3.zero;
            int litChecksCount = 0;
            foreach (Transform check in darknessStruggleChecks) {
                InDarkness darknessCheck = check.GetComponent<InDarkness>();
                darknessCheck.CheckDarkness();
                if (!darknessCheck.IsInDarkness) {
                    litChecksCount++;
                    positionSum += check.position;
                    Debug.Log(check.gameObject.name);
                }
            }
            
            if (litChecksCount == 0) {
                Debug.Log("Really stuck!!");
                if (TimeElapsedStuck != null) TimeElapsedStuck();
                else Debug.Log("Nothing to do about it!");
            } else {
                Debug.Log(litChecksCount + " out of 12 nearby areas lit");
                Vector3 average = positionSum / litChecksCount;
                Vector3 lightDirection = average - transform.position;
                rescueDirection = new Vector3(
                    unstuckForce * lightDirection.x,
                    0,
                    unstuckForce * lightDirection.z);
                InvokeRepeating("CheckSuccess", 0f, darknessCheck.checkInterval);
                SetStuckStatus(false); // force reset in case this needs to be repeated
            }
        }
    }

    private void CheckSuccess() {
        if (!darknessCheck.IsInDarkness) {
            CancelInvoke();
            rescueDirection = Vector3.zero;
            SetStuckStatus(false);
        } else {
            Vector3 move = Vector3.ProjectOnPlane(rescueDirection, character.groundNormal);
        }
    }

    void OnDrawGizmosSelected() {
        if (character == null) {
            return;
        }

        Vector3 move = Vector3.ProjectOnPlane(rescueDirection, character.groundNormal) / unstuckForce;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + move);
    }

    public void SetStuckStatus(bool stuck) {
        if (stuck ^ IsStuck) {
            IsStuck = stuck;
        }
    }

    public void UpdateStuckStatus() {
        if (approachingDarkness.IsActive ^ IsStuck) {
            IsStuck = approachingDarkness.IsActive;
        }
    }

    void Update() {
        UpdateStuckStatus();
    }
}

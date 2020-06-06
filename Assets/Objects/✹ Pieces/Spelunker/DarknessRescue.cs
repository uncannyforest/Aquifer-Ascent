using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class DarknessRescue : MonoBehaviour {
	public float stuckDelayFixTime = 3f;
	public float unstuckForce = 1f;

	private float struggleTime;

	private InDarkness successCheck;
	private Transform darknessStruggleChecks;
	private Rigidbody myRigidbody;
	private ThirdPersonCharacter characterScript;

	private Vector3 rescueDirection = Vector3.zero;

	/// Call only on state transition. Assumes previous state was opposite.
	public bool IsStuck {
		set {
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
		successCheck = transform.Find("DarknessCheck").GetComponent<InDarkness>();
		myRigidbody = GetComponent<Rigidbody>();
		characterScript = GetComponent<ThirdPersonCharacter>();
	}

	void FixedUpdate() {
		if (rescueDirection != Vector3.zero) {
			Vector3 move = Vector3.ProjectOnPlane(rescueDirection, characterScript.groundNormal);
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
				Debug.LogWarning("Really stuck!!");
			} else {
				Debug.Log(litChecksCount + " out of 12 nearby areas lit");
				Vector3 average = positionSum / litChecksCount;
				Vector3 lightDirection = average - transform.position;
				rescueDirection = new Vector3(
					unstuckForce * lightDirection.x,
					0,
					unstuckForce * lightDirection.z);
        		InvokeRepeating("CheckSuccess", 0f, successCheck.checkInterval);
				characterScript.isStuck = false; // force reset in case this needs to be repeated
			}
		}
	}

	private void CheckSuccess() {
		if (!successCheck.IsInDarkness) {
			CancelInvoke();
			rescueDirection = Vector3.zero;
			characterScript.isStuck = false;
			Debug.Log("Success!");
		} else {
			Vector3 move = Vector3.ProjectOnPlane(rescueDirection, characterScript.groundNormal);
			Debug.Log("Not there yet!");
			Debug.Log("rescueDirection: " + rescueDirection);
			Debug.Log("characterScript.groundNormal: " + characterScript.groundNormal);
			Debug.Log("move: " + move);
		}
	}

    void OnDrawGizmosSelected()
    {
		if (characterScript == null) {
			return;
		}

		Vector3 move = Vector3.ProjectOnPlane(rescueDirection, characterScript.groundNormal) / unstuckForce;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + move);
    }
}

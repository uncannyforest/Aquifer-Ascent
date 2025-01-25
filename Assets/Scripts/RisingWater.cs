using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RisingWater : MonoBehaviour {
    public Vector3 scale = new Vector3(.5f, 1, .5f);
    public float belowFactor = 1.5f;
    public float belowGuideFactor = 3f;

    private List<StandardOrb> orbs = new List<StandardOrb>();

    private Vector3 startLocation;
    private Vector3 endLocation;
    private Vector3 velocity;
    private Rigidbody myRigidbody;
    private float startChargeLevel;
    private float unchargeTime;
    public StandardOrb lowestOrb;
    public StandardOrb secondLowestOrb;
    public float currentLevelBelow;

    bool firstOrb = true;

    void Start() {
        startLocation = transform.position;
        endLocation = transform.position;
        myRigidbody = GetComponent<Rigidbody>(); // not used rn, causes logs to move horizontally with water flow
    }

    public void AddOrb(StandardOrb go) {
        go.died += () => OrbDied(go);
        orbs.Add(go);
        if (lowestOrb == null) SetLowestOrb();
    }

    public void OrbDied(StandardOrb go) {
        firstOrb = false;
        orbs.Remove(go);
        if (orbs.Count != 0) SetLowestOrb();
        else lowestOrb = null;
    }

    private void SetLowestOrb() {
        startLocation = transform.position.ScaleDivide(scale);
        if (firstOrb) {
            lowestOrb = orbs[0];
            startChargeLevel = lowestOrb.state.currentChargeLevel;
            currentLevelBelow = 0;
            return;
        }
        if (orbs.Count == 1) {
            lowestOrb = orbs[0];
            secondLowestOrb = orbs[0];
        } else if (orbs[0].transform.position.y < orbs[1].transform.position.y) {
            lowestOrb = orbs[0];
            secondLowestOrb = orbs[1];
        } else {
            lowestOrb = orbs[1];
            secondLowestOrb = orbs[0];
        }
        for (int i = 2; i < orbs.Count; i++) {
            StandardOrb orb = orbs[i];
            if (orb.transform.position.y < secondLowestOrb.transform.position.y) {
                if (orb.transform.position.y < lowestOrb.transform.position.y) {
                    secondLowestOrb = lowestOrb;
                    lowestOrb = orb;
                } else {
                    secondLowestOrb = orb;
                }
            }
        }
        StandardOrb lastOrb = orbs[orbs.Count - 1];
        currentLevelBelow = belowFactor * belowFactor / (belowFactor + (secondLowestOrb.transform.position.y - lowestOrb.transform.position.y))
            + orbs.Count == 1 ? 0 : belowGuideFactor * belowGuideFactor / (belowGuideFactor + (lastOrb.transform.position.y - lowestOrb.transform.position.y));
        startChargeLevel = lowestOrb.state.currentChargeLevel;
        unchargeTime = -lowestOrb.chargeTime * startChargeLevel;

        SetVelocity();
    }

    private void SetVelocity() {
        if (lowestOrb == null || unchargeTime == 0) {
            myRigidbody.velocity = Vector3.zero;
            return;
        }
        endLocation = lowestOrb.transform.position + currentLevelBelow * Vector3.down;
        velocity = (endLocation - startLocation) / unchargeTime;
        velocity.Scale(scale);
        Debug.Log("Set water velocity " + velocity + " from " + startLocation + " to reach endLocation " + endLocation + " in " + unchargeTime + " seconds");
        // myRigidbody.velocity = velocity;
    }

    void FixedUpdate() {
        if (!TimeTravel.I.timePaused)
            myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }
    // void FixedUpdate() {
    //     if (TimeTravel.I.timePaused) myRigidbody.velocity = Vector3.zero;
    //     else myRigidbody.velocity = velocity;
    // }
}

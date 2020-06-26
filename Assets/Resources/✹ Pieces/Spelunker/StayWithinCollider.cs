using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayWithinCollider : BooleanScript
{
    public GameObject stayWithin;

    /// if outside collider
    override public bool IsActive {
        get => isWithin.Count == 0;
    }

    private List<Collider> isWithin = new List<Collider>();

    void OnTriggerEnter(Collider other) {
        if(other.transform.parent.gameObject == stayWithin) {
            isWithin.Add(other);
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.transform.parent.gameObject == stayWithin) {
            isWithin.Remove(other);
        }
    }
}

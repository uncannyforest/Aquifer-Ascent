using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : MonoBehaviour {
    private Rigidbody myRigidbody;

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water")) {
            if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody>();
            if (myRigidbody != null && myRigidbody.isKinematic) myRigidbody.isKinematic = false;
        }
    }
}

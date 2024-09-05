using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : MonoBehaviour {
    public bool dropWhenAirborne = true;
    public AudioClip airborneSound;

    private Rigidbody myRigidbody;
    private AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water")) {
            if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody>();
            if (myRigidbody != null && myRigidbody.isKinematic) myRigidbody.isKinematic = false;
        }
    }

    public void Drop() {
        GetComponent<Holdable>().Drop();
        if (airborneSound != null) audioSource.PlayOneShot(airborneSound, .25f);
    }
}

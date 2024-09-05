using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VolumeRolloffY : MonoBehaviour {
    public float baseVolume = 1f;
    public float factor = .5f;
    public float updateTime = .25f;

    private AudioSource audioSource;
    private Transform audioListener;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        audioListener = FindObjectOfType<AudioListener>().transform;
        InvokeRepeating("UpdateDistance", 0, updateTime);
    }

    void UpdateDistance() {
        audioSource.volume = Mathf.Pow(factor, audioListener.position.y - audioSource.transform.position.y);
    }
}

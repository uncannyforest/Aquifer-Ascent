using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour {
    public float stopTimeDuration = 10;

    public void Add() {
        TimeTravel.I.TrySetReady(stopTimeDuration);
    }
}

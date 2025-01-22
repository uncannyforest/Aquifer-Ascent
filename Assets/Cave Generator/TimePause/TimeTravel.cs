using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravel : MonoBehaviour {
    private static TimeTravel instance;
    public static TimeTravel I { get => instance; }
    TimeTravel(): base() {
        instance = this;
    }

    public bool timePaused = false;
    public float timePausedFor = 0;
}

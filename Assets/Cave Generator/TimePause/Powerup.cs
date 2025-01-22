using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour {
    public float stopTimeDuration = 10;

    private bool added = false;

    public void Add() {
        FindObjectOfType<HoldObject>().inputDisplay.OverrideMessage2("stop time");
        added = true;
    }

    public void Use() {
        FindObjectOfType<HoldObject>().inputDisplay.OverrideMessage2(null);
        added = false;
        StartCoroutine(StopTime());
    } 

    // Update is called once per frame
    void Update() {
        if (added && SimpleInput.GetButtonDown("Interact2")) {
            Use();
        }
    }

    IEnumerator<YieldInstruction> StopTime() {
        TimeTravel.I.timePaused = true;
        TimeTravel.I.timePausedFor = stopTimeDuration;
        yield return new WaitForSeconds(stopTimeDuration);
        TimeTravel.I.timePaused = false;
    }
}

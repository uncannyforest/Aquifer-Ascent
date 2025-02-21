using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravel : MonoBehaviour {
    private static TimeTravel instance;
    public static TimeTravel I { get => instance; }
    TimeTravel(): base() {
        instance = this;
    }

    public ScarfMaterialsList offMaterials;
    public ScarfMaterialsList readyMaterials;
    public ScarfMaterialsList pausedMaterials;
    public bool timePaused;
    public float timePausedFor = 10;
    public bool ready;
    public float readyFor;
   
    private FlexibleInputDisplay inputDisplay;

    private Scarf scarf;
    void Start() {
        scarf = FindObjectOfType<Scarf>();
        inputDisplay = GetComponent<HoldObject>().inputDisplay;
    }

    public void TrySetReady(float duration) {
        ready = true;
        readyFor = duration;
        scarf.materials = readyMaterials;
        scarf.RefreshScarf();
        if (!timePaused) SetReady();
    }

    private void SetReady() {
        inputDisplay.OverrideMessage2("stop time");
    }

    void Update() {
        if (ready && !timePaused && !inputDisplay.message2OverrideBlocked && SimpleInput.GetButtonDown("Interact2")) Use();
    }
    public void Use() {
        inputDisplay.OverrideMessage2(null);
        ready = false;
        timePaused = true;
        timePausedFor = readyFor;
        scarf.materials = pausedMaterials;
        scarf.SwapScarf(true, out int _, out int __);
        StartCoroutine(Finish());
    } 

    IEnumerator<YieldInstruction> Finish() {
        yield return new WaitForSeconds(timePausedFor);
        timePaused = false;
        if (ready) SetReady();
        else SetOff();
    }

    private void SetOff() {
        scarf.materials = offMaterials;
        scarf.RefreshScarf();
    }

}

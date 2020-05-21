using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTrigger : MonoBehaviour {
    public BooleanScript trigger;
    public bool invert;
    public string result;
    public Animator receivingAnimator;

    private bool isActive = false;

    void Start() {
        TriggerAction(trigger.IsActive ^ invert);
        isActive = trigger.IsActive;
    }

    void Update() {
        if (isActive ^ trigger.IsActive) {
            TriggerAction(trigger.IsActive ^ invert);
            isActive = trigger.IsActive;
        }
    }

    private void TriggerAction(bool input) {
        if (receivingAnimator != null) {
            receivingAnimator.SetBool(result, input);
        }
    }
}

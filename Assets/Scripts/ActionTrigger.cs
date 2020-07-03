using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTrigger : MonoBehaviour {
    public BooleanScript trigger;
    public bool invert;
    public List<BooleanInput> additonalTriggerInputs = new List<BooleanInput>();
    public string result;
    public Animator receivingAnimator;
    public List<ToggleableScript> receivingScripts;

    private bool isActive = false;

    void Start() {
        isActive = ComputeAllInput();
        TriggerAction(isActive);
    }

    void Update() {
        bool newIsActive = ComputeAllInput();
        if (isActive ^ newIsActive) {
            TriggerAction(newIsActive);
            isActive = newIsActive;
        }
    }

    private void TriggerAction(bool input) {
        if (receivingAnimator != null) {
            receivingAnimator.SetBool(result, input);
        }
        foreach (ToggleableScript receivingScript in receivingScripts) {
            receivingScript.IsActive = input;
        }
    }

    private bool ComputeOneInput(BooleanScript trigger, bool invert) {
        return trigger.IsActive ^ invert;
    }

    private bool ComputeAllInput() {
        bool result = ComputeOneInput(trigger, invert);
        foreach (BooleanInput input in additonalTriggerInputs) {
            switch(input.operation) {
            case BooleanInput.Operation.And:
                result = result && ComputeOneInput(input.trigger, input.invert);
                break;
            case BooleanInput.Operation.Or:
                result = result || ComputeOneInput(input.trigger, input.invert);
                break;
            case BooleanInput.Operation.Xor:
                result = result ^ ComputeOneInput(input.trigger, input.invert);
                break;
            }
        }
        return result;
    }

    [Serializable]
    public class BooleanInput {
        public Operation operation;
        public BooleanScript trigger;
        public bool invert;

        public enum Operation { And, Or, Xor }
    }
}

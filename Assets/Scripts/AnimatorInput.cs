using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorInput : BooleanScript {
    public Animator sendingAnimator;
    public int layerIndex;
    public string animatorState;

    override public bool IsActive {
        get {
            AnimatorStateInfo stateInfo = sendingAnimator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.IsName(animatorState) && !sendingAnimator.IsInTransition(layerIndex);
        }
    }
}

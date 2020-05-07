using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> holds code governing hold animation behavior </summary>
public class HoldAnimationControl {
    public float heldObjectWidth;

    private HoldObject script;
    private Transform playerHoldTransform;

    private int stoopAnimationLayer = 1;
    private string stoopStateName = "Stoop";

    private Animator animator;
    private Vector3 objectGroundPosition;
    private Vector3 defaultPlayerHoldPosition;
    private float handWeightCurrent = 0;

    public HoldAnimationControl(HoldObject script, Transform playerHoldTransform) {
        this.script = script;
        this.playerHoldTransform = playerHoldTransform;
        defaultPlayerHoldPosition = playerHoldTransform.localPosition;
        animator = script.GetComponent<Animator>();
    }

    /// <return> ground object </return>
    public Transform StartStoop() {
        animator.SetTrigger(stoopStateName);
        Physics.Raycast(playerHoldTransform.position, Vector3.down, out RaycastHit ground);
        objectGroundPosition = script.transform.InverseTransformPoint(ground.point);
        return ground.transform;
    }
    
    public void ResetPlayerHoldTransform() {
        playerHoldTransform.localPosition = defaultPlayerHoldPosition;
    }

    public void UpdatePlayerHoldTransform() {
         // IsHolding means still first half, going down
        if (script.isTransitioning == HoldObject.Transition.Dropping && script.IsHolding) {
            float stoopDownwardProgress;
            if (animator.GetCurrentAnimatorStateInfo(stoopAnimationLayer).IsName(stoopStateName)) {
                stoopDownwardProgress =
                    animator.GetCurrentAnimatorStateInfo(stoopAnimationLayer).normalizedTime * 2;
            } else { // The transition has just begun, Stoop is in progress but as the "next state"
                stoopDownwardProgress =
                    animator.GetNextAnimatorStateInfo(stoopAnimationLayer).normalizedTime * 2;
            }
            playerHoldTransform.localPosition =
                    Vector3.Lerp(defaultPlayerHoldPosition, objectGroundPosition, stoopDownwardProgress);
        }
    }

    public void AnimatorIK() {
        if(!script.IsHolding && handWeightCurrent == 0){
            return;
        }

        Vector3 rightHandlePosition = playerHoldTransform.position + (.5f * heldObjectWidth * script.transform.right);
        Vector3 leftHandlePosition = playerHoldTransform.position - (.5f * heldObjectWidth * script.transform.right);

        if(script.IsHolding) { // if you're holding something 
            if( handWeightCurrent < 1f ){
                handWeightCurrent += Time.deltaTime / script.transitionTime;
            }else{
                handWeightCurrent = 1f;
            }
        }else{
            // Let the hands relax :)

            if( handWeightCurrent > 0f ){
                handWeightCurrent -= Time.deltaTime / script.transitionTime;
            }else{
                handWeightCurrent = 0f;
            }
        }

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand,handWeightCurrent);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand,handWeightCurrent); 	
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,handWeightCurrent);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,handWeightCurrent); 

        animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandlePosition);
        animator.SetIKRotation(AvatarIKGoal.RightHand,playerHoldTransform.rotation);
        animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandlePosition);
        animator.SetIKRotation(AvatarIKGoal.LeftHand,playerHoldTransform.rotation);  
    }
}

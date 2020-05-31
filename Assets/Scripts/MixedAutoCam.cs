using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR

#endif

namespace UnityStandardAssets.Cameras
{
    [ExecuteInEditMode]
    public class MixedAutoCam : PivotBasedCameraRig
    {
        [SerializeField] private float m_MoveSpeed = 3; // How fast the rig will move to keep up with target's position
        [SerializeField] private float m_TurnSpeed = 1; // How fast the rig will turn to keep up with target's rotation
        [SerializeField] private float m_RollSpeed = 0.2f;// How fast the rig will roll (around Z axis) to match target's roll.
        [SerializeField] private bool m_FollowTilt = true; // Whether the rig will tilt (around X axis) with the target.
        [SerializeField] private float m_TargetVelocityLowerLimit = 4f;// the minimum velocity above which the camera turns towards the object's velocity.
        [SerializeField] private float m_SmoothTurnTime = 0.2f; // the smoothing for the camera's rotation
        [SerializeField] private float m_StraightOnAngle = 20; // Angle to not move camera when facing the camera
        [SerializeField] private float m_ResetTurnSpeed = 8; // How fast the rig will turn when resetting, 0 to disable

        private float m_LastFlatAngle; // The relative angle of the target and the rig from the previous frame.
        private float m_CurrentTurnAmount; // How much to turn the camera
        private float m_TurnSpeedVelocityChange; // The change in the turn speed velocity
        private Vector3 m_RollUp = Vector3.up;// The roll of the camera around the z axis ( generally this will always just be up )

        private bool isResettingCamera = false;

        public bool IsDoubleResetReady {
            get {
                Quaternion rollRotation = Quaternion.LookRotation(m_Target.forward, m_RollUp);
                bool isFacingForward = Quaternion.Angle(transform.rotation, rollRotation) < m_StraightOnAngle;
                return isResettingCamera || isFacingForward;
            }
        }

        protected void Update()
        {
            if (SimpleInput.GetButtonDown("Reset Camera") && m_ResetTurnSpeed > 0) {
                isResettingCamera = true;
            }
        }

        protected override void FollowTarget(float deltaTime)
        {
            // if no target, or no time passed then we quit early, as there is nothing to do
            if (!(deltaTime > 0) || m_Target == null)
            {
                return;
            }

            // initialise some vars, we'll be modifying these in a moment
            var targetForward = m_Target.forward;
            var targetUp = m_Target.up;

            if (Application.isPlaying)
            {
                if (isResettingCamera) {
                    targetForward = m_Target.forward;
                    targetUp = m_Target.up;
                    m_CurrentTurnAmount = Mathf.SmoothDamp(m_CurrentTurnAmount, 1, ref m_TurnSpeedVelocityChange, m_SmoothTurnTime);
                }
                // the camera's rotation is aligned towards the object's velocity direction
                // but only if the object is traveling faster than a given threshold.
                else if (targetRigidbody.velocity.magnitude > m_TargetVelocityLowerLimit)
                {
                    // velocity is high enough, so we'll use the target's velocty
                    targetForward = targetRigidbody.velocity.normalized;
                    targetUp = Vector3.up;
                    m_CurrentTurnAmount = Mathf.SmoothDamp(m_CurrentTurnAmount, 1, ref m_TurnSpeedVelocityChange, m_SmoothTurnTime);
                }
                else
                {
                    targetUp = Vector3.up;
                    m_CurrentTurnAmount = 0;
                }
            }

            // camera position moves towards target position:
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime*m_MoveSpeed);

            // camera's rotation is split into two parts, which can have independend speed settings:
            // rotating towards the target's forward direction (which encompasses its 'yaw' and 'pitch')
            if (!m_FollowTilt)
            {
                targetForward.y = 0;
                if (targetForward.sqrMagnitude < float.Epsilon)
                {
                    targetForward = transform.forward;
                }
            }
            var rollRotation = Quaternion.LookRotation(targetForward, m_RollUp);

            // and aligning with the target object's up direction (i.e. its 'roll')
            m_RollUp = m_RollSpeed > 0 ? Vector3.Slerp(m_RollUp, targetUp, m_RollSpeed*deltaTime) : Vector3.up;
            if(!isResettingCamera && Quaternion.Angle(transform.rotation, rollRotation) > 180 - m_StraightOnAngle) {
                return; // Don't adjust angle, walking backwards                
            }
            float turnSpeedToUse = isResettingCamera ? m_ResetTurnSpeed : m_TurnSpeed;
            transform.rotation = Quaternion.Lerp(transform.rotation, rollRotation, turnSpeedToUse*m_CurrentTurnAmount*deltaTime);

            if (isResettingCamera && Quaternion.Angle(rollRotation, transform.rotation) < 2) {
                isResettingCamera = false;
            }
        }
    }
}

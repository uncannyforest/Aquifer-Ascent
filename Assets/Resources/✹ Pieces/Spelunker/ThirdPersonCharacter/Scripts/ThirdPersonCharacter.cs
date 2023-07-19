using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] PhysicMaterial m_StationaryMaterial;
		[SerializeField] PhysicMaterial m_MovingMaterial;
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] bool m_TrailMix = false;
		[SerializeField] float m_JumpPower = 12f;
		[SerializeField] float m_ForwardJumpPower = 2.5f;
		[Range(0f, 4f)][SerializeField] public float m_GroundGravity = 1f;
		[Range(0f, 4f)][SerializeField] public float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.125f; // This distance and radius make 
		[SerializeField] float m_GroundCheckRadius = 0.05f;    // slopes walkable up to approx. 45 deg
		[SerializeField] float m_JumpMinNotGroundedTime = 0.1f;
		[SerializeField] List<BooleanScript> m_ProhibitMotionWhen;

		[NonSerialized] public Vector3 groundNormal;

		int m_CollidingLayerMask;
		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_NextGroundCheckAfterJump = 0;
		float m_ActualGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		CapsuleCollider m_Capsule;

		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();

			m_CollidingLayerMask = GetCollidingLayerMask();

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_ActualGroundCheckDistance = m_GroundCheckDistance;
		}

		public void Move(Vector3 move, bool jump) {
			Vector3 forwardPush = move;

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			if (Time.time > m_NextGroundCheckAfterJump) CheckGroundStatus();
			float moveMag = move.magnitude;
			move = Vector3.ProjectOnPlane(move, groundNormal);
			if (m_TrailMix) move = move.normalized * Mathf.Sqrt(moveMag);
			m_TurnAmount = Mathf.Atan2(move.x, move.z);
			m_ForwardAmount = move.z;
			// Debug.Log("Forward " + m_ForwardAmount);

			ApplyExtraTurnRotation();

			if (move.magnitude > 0 || !m_IsGrounded) {
				m_Capsule.material = m_MovingMaterial;
			} else {
				m_Capsule.material = m_StationaryMaterial;
			}

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded) {
				HandleGroundedMovement(forwardPush, jump);
				m_Rigidbody.AddForce(Physics.gravity * m_GroundGravity);
			} else {
				HandleAirborneMovement();
			}

			// send input and other state parameters to the animator
			UpdateAnimator(move);
		}

		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			m_Animator.SetBool("OnGround", m_IsGrounded);
			if (!m_IsGrounded)
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
			if (m_IsGrounded)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0) {
				m_Animator.speed = m_AnimSpeedMultiplier;
			} else {
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}


		void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier);
			m_Rigidbody.AddForce(extraGravityForce);

			if (MovementIsProhibited) {
				m_Rigidbody.velocity = new Vector3(0, m_Rigidbody.velocity.y, 0);
			}

			m_ActualGroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_GroundCheckDistance : 0.01f;
		}


		void HandleGroundedMovement(Vector3 forwardPush, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
				// jump!
				Vector3 jumpPush = forwardPush * m_ForwardJumpPower;
				Vector3 newVelocity;

				Vector3 oldVelocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
				float newSpeed = oldVelocity.magnitude >= jumpPush.magnitude * 2 ?
					oldVelocity.magnitude : oldVelocity.magnitude / 2 + jumpPush.magnitude;
				Vector3 newDirection = oldVelocity + jumpPush;
				newVelocity = newDirection.normalized * newSpeed;
				// Debug.Log("Original velocity: " + oldVelocity);
				// Debug.Log("Forward push: " + jumpPush);
				// Debug.Log("New velocity: " + newVelocity);
				
				m_Rigidbody.velocity = new Vector3(newVelocity.x, m_JumpPower, newVelocity.z);
				m_IsGrounded = false;
				m_NextGroundCheckAfterJump = Time.time + m_JumpMinNotGroundedTime;
				m_Animator.applyRootMotion = false;
				m_ActualGroundCheckDistance = 0.1f;
			}
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}


		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (m_IsGrounded && Time.deltaTime > 0)
			{
				Vector3 v = MovementIsProhibited ? Vector3.zero : (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = m_Rigidbody.velocity.y;
				m_Rigidbody.velocity = v;
			}
		}

		void OnDrawGizmos() {
			Gizmos.DrawSphere(transform.position + (Vector3.up * (0.1f - m_ActualGroundCheckDistance)), m_GroundCheckRadius);
		}
 
		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_ActualGroundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			bool sphereCast = Physics.SphereCast(
				transform.position + (Vector3.up * (0.1f + m_GroundCheckRadius)),
				m_GroundCheckRadius,
				Vector3.down,
				out hitInfo,
				m_ActualGroundCheckDistance + m_GroundCheckRadius,
				m_CollidingLayerMask);
			// m_RunningAvgGroundSlope = m_RunningAvgGroundSlope < slope ?
			// 	 (m_RunningAvgGroundSlope * (m_SlopeCheckSmoother - 1) + slope) / m_SlopeCheckSmoother : slope;
			// Debug.Log("Slope: " + (1 - hitInfo.normal.y) + " / new running avg:" + m_RunningAvgGroundSlope);
			if (sphereCast)
			{
				groundNormal = hitInfo.normal;
				m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
			}
			else
			{
				m_IsGrounded = false;
				groundNormal = sphereCast ? hitInfo.normal : Vector3.up;
				m_Animator.applyRootMotion = false;
			}
		}

		private int GetCollidingLayerMask() {
			int layerMask = 0;
			for (int i = 0; i < 32; i++) {
				if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i)) {
					layerMask |= 1 << i;
				}
			}
			return layerMask;
		}

		private bool MovementIsProhibited {
			get {
				foreach (BooleanScript script in m_ProhibitMotionWhen)
					if (script.isActiveAndEnabled && script.IsActive)
						return true;
				return false;
			}
		}
	}
}

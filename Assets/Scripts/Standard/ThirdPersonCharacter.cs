using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(InDarkness))]
	[RequireComponent(typeof(HoldObject))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] PhysicMaterial m_StationaryMaterial;
		[SerializeField] PhysicMaterial m_MovingMaterial;
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 12f;
		[SerializeField] float m_ForwardJumpPower = 2.5f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_BaseMoveSpeedMultiplier = 1f;
		[SerializeField] float m_HoldingAnimSpeedMultiplier = 1f;
		[SerializeField] float m_DarknessAnimSpeedMultiplier = 0.4f;
		[SerializeField] float m_GroundCheckDistance = 0.1f;

		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		CapsuleCollider m_Capsule;
		float m_CapsuleRadius;
		InDarkness m_DarknessCheck;
		HoldObject m_HoldScript;

		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleRadius = m_Capsule.radius;
			m_DarknessCheck = transform.Find("DarknessCheck").GetComponent<InDarkness>();
			m_HoldScript = GetComponent<HoldObject>();

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;
		}


		public void Move(Vector3 move, bool jump) {
			Vector3 forwardPush = move;

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, m_GroundNormal);
			m_TurnAmount = Mathf.Atan2(move.x, move.z);
			m_ForwardAmount = move.z;

			ApplyExtraTurnRotation();

			if (move.magnitude > 0) {
				m_Capsule.material = m_MovingMaterial;
			} else {
				m_Capsule.material = m_StationaryMaterial;
			}

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded) {
				HandleGroundedMovement(forwardPush, jump);
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
			if (m_IsGrounded && m_DarknessCheck.IsInDarkness) {
				m_Animator.speed = m_DarknessAnimSpeedMultiplier;
			} else if (m_IsGrounded && move.magnitude > 0) {
				if (m_HoldScript.IsHolding) {
					m_Animator.speed = m_HoldingAnimSpeedMultiplier;
				} else {
					m_Animator.speed = m_AnimSpeedMultiplier;
				}
			} else {
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}


		void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			m_Rigidbody.AddForce(extraGravityForce);

			m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
		}


		void HandleGroundedMovement(Vector3 forwardPush, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
				// jump!
				Vector3 jumpPush = forwardPush * m_ForwardJumpPower;
				Vector3 newVelocity;
				if (m_DarknessCheck.IsInDarkness) {
					newVelocity = jumpPush;
				} else {
					Vector3 oldVelocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
					float newSpeedSquared = oldVelocity.sqrMagnitude
						+ jumpPush.sqrMagnitude;
					Vector3 newDirection = oldVelocity + jumpPush;
					newVelocity = newDirection.normalized * Mathf.Sqrt(newSpeedSquared);
					Debug.Log("Original velocity:" + oldVelocity.magnitude);
					Debug.Log("Forward push:" + jumpPush.magnitude);
					Debug.Log("New velocity:" + newVelocity.magnitude);
				}
				
				m_Rigidbody.velocity = new Vector3(newVelocity.x, m_JumpPower, newVelocity.z);
				m_IsGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
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
				float actualMoveSpeedMultiplier;
				if (!m_DarknessCheck.IsInDarkness && !m_HoldScript.IsHolding) {
					actualMoveSpeedMultiplier = m_MoveSpeedMultiplier;
				} else {
					actualMoveSpeedMultiplier = m_BaseMoveSpeedMultiplier;
				}

				Vector3 v = (m_Animator.deltaPosition * actualMoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = m_Rigidbody.velocity.y;
				m_Rigidbody.velocity = v;
			}
		}


		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.SphereCast(transform.position + (Vector3.up * (0.1f + m_CapsuleRadius)), m_CapsuleRadius, Vector3.down, out hitInfo, m_GroundCheckDistance))
			{
				m_GroundNormal = hitInfo.normal;
				m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
			}
			else
			{
				m_IsGrounded = false;
				m_GroundNormal = Vector3.up;
				m_Animator.applyRootMotion = false;
			}
		}
	}
}

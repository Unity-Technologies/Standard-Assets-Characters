using System;
using StandardAssets.Characters.Input;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// The main third person controller
	/// </summary>
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public class PhysicsThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		/// <summary>
		/// Movement values
		/// </summary>
		public Transform    cameraTransform;
		public float        maxForwardSpeed             = 10f;
		public bool         useAcceleration             = true;
		public float        groundAcceleration          = 20f;
		public float        groundDeceleration          = 15f;
		[Range (0f, 1f)]
		public float        airborneAccelProportion     = 0.5f;
		[Range (0f, 1f)] 
		public float        airborneDecelProportion     = 0.5f;
		public float        jumpSpeed                   = 15f;
		public bool         interpolateTurning          = true;
		public float        turnSpeed                   = 500f;
		[Range (0f, 1f)] 
		public float        airborneTurnSpeedProportion = 0.5f;
		
		/// <inheritdoc />
		public float turningSpeed { get; private set; }
		
		/// <inheritdoc />
		public float lateralSpeed { get; private set; }
		
		/// <inheritdoc />
		public float forwardSpeed { get; private set; }

		public Action jumpStart { get; set; }
		public Action lands { get; set; }

		/// <summary>
		/// The input implementation
		/// </summary>
		ICharacterInput m_CharacterInput;
		
		/// <summary>
		/// The physic implementation
		/// </summary>
		ICharacterPhysics m_CharacterPhysics;

		/// <summary>
		/// Gets required components
		/// </summary>
		void Awake()
		{
			m_CharacterInput = GetComponent<ICharacterInput>();
			m_CharacterInput.jump += OnJump;
			m_CharacterPhysics = GetComponent<ICharacterPhysics>();
			m_CharacterPhysics.lands += OnLand;
		}

		/// <summary>
		/// Handles player landing
		/// </summary>
		void OnLand()
		{
			if (lands != null)
			{
				lands();
			}
		}

		/// <summary>
		/// Subscribes to the Jump action on input
		/// </summary>
		void OnJump()
		{
			if (m_CharacterPhysics.isGrounded)
			{
				m_CharacterPhysics.Jump(jumpSpeed);
				if (jumpStart != null)
				{
					jumpStart();
				}
			}
		}

		/// <summary>
		/// Movement Logic on physics update
		/// </summary>
		void FixedUpdate ()
		{
			SetForward ();
			CalculateForwardMovement ();
			Move();
			
			
		}

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		void SetForward()
		{
			if (!m_CharacterInput.hasMovementInput)
			{
				return;
			}
			
			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection = new Vector3(m_CharacterInput.moveInput.x, 0f, m_CharacterInput.moveInput.y);
            
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);

			if (interpolateTurning)
			{
				float actualTurnSpeed = m_CharacterPhysics.isGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
				targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.deltaTime);
			}

			transform.rotation = targetRotation;
		}

		/// <summary>
		/// Calculates the forward movement
		/// </summary>
		void CalculateForwardMovement()
		{
			Vector2 moveInput = m_CharacterInput.moveInput;
			if (moveInput.sqrMagnitude > 1f)
			{
				moveInput.Normalize();
			}

			float desiredSpeed = moveInput.magnitude * maxForwardSpeed;

			if (useAcceleration)
			{
				float acceleration = m_CharacterPhysics.isGrounded
					? (m_CharacterInput.hasMovementInput ? groundAcceleration : groundDeceleration)
					: (m_CharacterInput.hasMovementInput ? groundAcceleration : groundDeceleration) * airborneDecelProportion;

				forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * Time.deltaTime);
			}
			else
			{
				forwardSpeed = desiredSpeed;
			}
		}

		/// <summary>
		/// Moves the character
		/// </summary>
		void Move()
		{
			Vector3 movement;
			//TODO: clean-up
//			if (m_IsGrounded && m_Animator.deltaPosition.z >= groundAcceleration * Time.deltaTime)
//			{
//				RaycastHit hit;
//				Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
//				if (Physics.Raycast (ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
//				{
//					movement = Vector3.ProjectOnPlane (m_Animator.deltaPosition, hit.normal);
//				}
//				else
//				{
//					movement = m_Animator.deltaPosition;
//				}
//			}
//			else
//			{
				movement = forwardSpeed * transform.forward * Time.deltaTime;
//			}

			//movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

			m_CharacterPhysics.Move(movement);
		}

		

	}
}

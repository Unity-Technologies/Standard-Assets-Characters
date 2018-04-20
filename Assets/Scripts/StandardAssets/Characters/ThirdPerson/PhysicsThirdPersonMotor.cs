using StandardAssets.Characters.Input;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(IPhysics))]
	[RequireComponent(typeof(IThirdPersonInput))]
	public class PhysicsThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		#region Inspector
		
		public Transform    cameraTransform;
		public float        maxForwardSpeed             = 10f;
		public bool         useAcceleration             = true;
		public float        groundAcceleration          = 20f;
		public float        groundDeceleration          = 15f;
		[Range (0f, 1f)]
		public float        airborneAccelProportion     = 0.5f;
		[Range (0f, 1f)] 
		public float        airborneDecelProportion     = 0.5f;
		public float        gravity                     = 10f;
		public float        jumpSpeed                   = 15f;
		public bool         interpolateTurning          = true;
		public float        turnSpeed                   = 500f;
		[Range (0f, 1f)] 
		public float        airborneTurnSpeedProportion = 0.5f;
		
		#endregion

		#region Properties
		
		public float turningSpeed { get; private set; }
		public float lateralSpeed { get; private set; }
		public float forwardSpeed { get; private set; }
		
		#endregion


		#region Required Components
		
		IThirdPersonInput m_Input;
		IPhysics m_Physics;
		
		#endregion
		
		bool m_IsGrounded    = true;

		void Awake()
		{
			m_Input = GetComponent<IThirdPersonInput>();
			m_Physics = GetComponent<IPhysics>();
		}
		
		void FixedUpdate ()
		{
			SetForward ();
			CalculateForwardMovement ();
//			CalculateVerticalMovement ();
			//SetNormalisedTime ();
			Move();
		}


		void SetForward()
		{
			if (!m_Input.isMoveInput)
			{
				return;
			}
			
			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection = new Vector3(m_Input.moveInput.x, 0f, m_Input.moveInput.y);
            
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);

			if (interpolateTurning)
			{
				float actualTurnSpeed = m_IsGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
				targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.deltaTime);
			}

			transform.rotation = targetRotation;
		}

		void CalculateForwardMovement()
		{
			Vector2 moveInput = m_Input.moveInput;
			if (moveInput.sqrMagnitude > 1f)
			{
				moveInput.Normalize();
			}

			float desiredSpeed = moveInput.magnitude * maxForwardSpeed;

			if (useAcceleration)
			{
				float acceleration = m_IsGrounded
					? (m_Input.isMoveInput ? groundAcceleration : groundDeceleration)
					: (m_Input.isMoveInput ? groundAcceleration : groundDeceleration) * airborneDecelProportion;

				forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * Time.deltaTime);
			}
			else
			{
				forwardSpeed = desiredSpeed;
			}
		}

//		void CalculateVerticalMovement()
//		{
//			throw new System.NotImplementedException();
//		}
		
		void Move()
		{
			Vector3 movement;
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

			m_Physics.Move(movement);

//			m_IsGrounded = m_CharCtrl.isGrounded;
//			m_Animator.SetBool(m_HashGroundedPara, m_IsGrounded);
		}
	}
}

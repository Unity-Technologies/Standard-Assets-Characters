using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A physic implementation that uses the default Unity character controller
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class CharacterControllerCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		/// <summary>
		/// The value of gravity
		/// </summary>
		public float gravity;

		/// <summary>
		/// The maximum speed that the character can move downwards
		/// </summary>
		public float terminalVelocity = 10f;

		/// <summary>
		/// The distance used to check if grounded
		/// </summary>
		public float groundCheckDistance = 0.51f;

		/// <summary>
		/// Layers to use in the ground check
		/// </summary>
		[Tooltip("Layers to use in the ground check")]
		public LayerMask groundCheckMask;

		/// <summary>
		/// Character controller
		/// </summary>
		private CharacterController characterController;
		
		/// <summary>
		/// The initial jump velocity
		/// </summary>
		private float initialJumpVelocity;

		/// <summary>
		/// The current vertical velocity
		/// </summary>
		/// <returns></returns>
		private float currentVerticalVelocity;
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		private Vector3 verticalVector = Vector3.zero;

		/// <summary>
		/// Stores the grounded-ness of the physics object
		/// </summary>
		private bool grounded;
		
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

		/// <inheritdoc />
		public bool isGrounded
		{
			get { return grounded; }
		}
		
		/// <inheritdoc />
		public void Move(Vector3 moveVector3)
		{
			
			characterController.Move(moveVector3 + verticalVector);
		}

		/// <summary>
		/// Tries to jump
		/// </summary>
		/// <param name="initialVelocity"></param>
		public void SetJumpVelocity(float initialVelocity)
		{
			initialJumpVelocity = initialVelocity;
			if (jumpVelocitySet != null)
			{
				jumpVelocitySet();
			}
		}

		private void Awake()
		{
			//Gets the attached character controller
			characterController = GetComponent<CharacterController>();
			
			//Ensures that the gravity acts downwards
			if (gravity > 0)
			{
				gravity = -gravity;
			}

			if (terminalVelocity > 0)
			{
				terminalVelocity = -terminalVelocity;
			}
		}

		/// <summary>
		/// Handle falling physics
		/// </summary>
		private void FixedUpdate()
		{
			AerialMovement();
		}
		
		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		private void AerialMovement()
		{
			grounded = CheckGrounded();
			
			airTime += Time.fixedDeltaTime;
			currentVerticalVelocity = Mathf.Clamp(initialJumpVelocity + gravity * airTime, terminalVelocity, Mathf.Infinity);
			float previousFallTime = fallTime;

			if (currentVerticalVelocity < 0)
			{
				fallTime += Time.fixedDeltaTime;
			}
			
			if (currentVerticalVelocity < 0f && grounded)
			{
				initialJumpVelocity = 0f;
				verticalVector = Vector3.zero;
				
				//Play the moment that the character lands and only at that moment
				if (Math.Abs(airTime - Time.fixedDeltaTime) > Mathf.Epsilon)
				{
					if (landed != null)
					{
						landed();
					}
				}

				fallTime = 0f;
				airTime = 0f;
				return;
			}
			
			if (previousFallTime < Mathf.Epsilon && fallTime > Mathf.Epsilon)
			{
				if (startedFalling != null)
				{
					startedFalling();
				}
			}
			
			verticalVector = new Vector3(0, currentVerticalVelocity, 0);
		}
		
		/// <summary>
		/// Checks character controller grounding
		/// </summary>
		private bool CheckGrounded()
		{
			Debug.DrawRay(transform.position + characterController.center, new Vector3(0,-groundCheckDistance * characterController.height,0), Color.red);
			if (UnityEngine.Physics.Raycast(transform.position + characterController.center, 
				-transform.up, groundCheckDistance * characterController.height, groundCheckMask))
			{
				return true;
			}
			return CheckEdgeGrounded();
			
		}

		/// <summary>
		/// Checks character controller edges for ground
		/// </summary>
		private bool CheckEdgeGrounded()
		{
			
			Vector3 xRayOffset = new Vector3(characterController.radius,0f,0f);
			Vector3 zRayOffset = new Vector3(0f,0f,characterController.radius);		
			
			for (int i = 0; i < 4; i++)
			{
				float sign = 1f;
				Vector3 rayOffset;
				if (i % 2 == 0)
				{
					rayOffset = xRayOffset;
					sign = i - 1f;
				}
				else
				{
					rayOffset = zRayOffset;
					sign = i - 2f;
				}
				Debug.DrawRay(transform.position + characterController.center + sign * rayOffset, 
					new Vector3(0,-groundCheckDistance * characterController.height,0), Color.blue);

				if (UnityEngine.Physics.Raycast(transform.position + characterController.center + sign * rayOffset,
					-transform.up,groundCheckDistance * characterController.height, groundCheckMask))
				{
					return true;
				}
			}
			return false;
		}
	}
}
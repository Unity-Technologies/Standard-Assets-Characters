using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A physic implementation that uses the default Unity character controller
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class CharacterControllerCharacterPhysics : BaseCharacterPhysics
	{
		/// <summary>
		/// The distance used to check if grounded
		/// </summary>
		[SerializeField]
		private float groundCheckDistance = 0.51f;

		/// <summary>
		/// Layers to use in the ground check
		/// </summary>
		[Tooltip("Layers to use in the ground check")]
		[SerializeField]
		private LayerMask groundCheckMask;

		/// <summary>
		/// Character controller
		/// </summary>
		private CharacterController characterController;

		protected override void Awake()
		{
			//Gets the attached character controller
			characterController = GetComponent<CharacterController>();
			base.Awake();
		}

		/// <summary>
		/// Checks character controller grounding
		/// </summary>
		protected override bool CheckGrounded()
		{
			Debug.DrawRay(transform.position + characterController.center, new Vector3(0,-groundCheckDistance * characterController.height,0), Color.red);
			if (UnityEngine.Physics.Raycast(transform.position + characterController.center, 
				-transform.up, groundCheckDistance * characterController.height, groundCheckMask))
			{
				return true;
			}
			return CheckEdgeGrounded();
			
		}

		protected override void MoveCharacter(Vector3 movement)
		{
			characterController.Move(movement);
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
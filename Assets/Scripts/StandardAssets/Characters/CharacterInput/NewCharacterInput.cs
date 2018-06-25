using System;
using System.Security.Cryptography;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.UI;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Character Input using the experimental input system.
	/// </summary>
	public class NewCharacterInput : MonoBehaviour, ICharacterInput
	{
		[SerializeField]
		private GameplayInputActions gameplay;
		
		[SerializeField]
		private InputActionReference[] lookActionReferences;

		private Vector2 look;

		public Vector2 moveInput { get; private set; }
		
		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}

		public Action jumpPressed { get; set; }

		void Awake()
		{
			//controls.Enable();
		
		
			
		}
	
		
		public void OnEnable()
		{
			gameplay.Enable();
			gameplay.controls.movement.performed += Move;
			gameplay.controls.jump.performed += Jump;
			
			foreach (InputActionReference inputActionReference in lookActionReferences)
			{
				inputActionReference.action.performed += Look;
			}
			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		public void OnDisable()
		{
			gameplay.Disable();
			gameplay.controls.movement.performed -= Move;
			gameplay.controls.jump.performed -= Jump;
		
			foreach (InputActionReference inputActionReference in lookActionReferences)
			{
				inputActionReference.action.performed -= Look;
			}
		}

		private void Move(InputAction.CallbackContext ctx)
		{
			moveInput = ctx.ReadValue<Vector2>();
			float rawMoveX = moveInput.x;
		}

		private void Look(InputAction.CallbackContext ctx)
		{
			look = ctx.ReadValue<Vector2>();	
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string axis)
		{
			if (axis == "Mouse X")
			{
				return look.x;
			}

			if (axis == "Mouse Y")
			{
				return look.y;
			}

			return 0;
		}

		private void Jump(InputAction.CallbackContext ctx)
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}	
		}
		
	}
	
}

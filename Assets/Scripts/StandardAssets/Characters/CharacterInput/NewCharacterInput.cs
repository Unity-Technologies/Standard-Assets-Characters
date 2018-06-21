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
		private GameplayInputActions controls;
		
		//[SerializeField]
		//private NewInputActions controls;
		
		[SerializeField]
		private InputActionReference[] lookActionReferences;

		private Vector2 look;

		public Vector2 moveInput { get; private set; }
		
		//StickDebug
		public Text leftStickText;
		public Text rightStickText;

		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}

		public Action jumpPressed { get; set; }

		void Awake()
		{
			/*
			 * controls.gameplay.movement.AppendCompositeBinding("Dpad")
				.With("Left", "<Keyboard>/a")
				.With("Right", "<Keyboard>/d")
				.With("Up", "<Keyboard>/w")
				.With("Down", "<Keyboard>/s");
			 */
		}
		
		
		public void OnEnable()
		{
			controls.Enable();
			//

			controls.gameplay.movement.performed += Move;
			foreach (InputActionReference inputActionReference in lookActionReferences)
			{
				inputActionReference.action.performed += Look;
			}
			controls.gameplay.jump.performed += Jump;

		

			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		public void OnDisable()
		{
			controls.Disable();
			controls.gameplay.movement.performed -= Move;
			foreach (InputActionReference inputActionReference in lookActionReferences)
			{
				inputActionReference.action.performed -= Look;
			}
			controls.gameplay.jump.performed -= Jump;
			

		}

		private void Move(InputAction.CallbackContext ctx)
		{
			moveInput = ctx.ReadValue<Vector2>();
			float rawMoveX = moveInput.x;
			leftStickText.text = rawMoveX.ToString();
		}

		private void Look(InputAction.CallbackContext ctx)
		{
			look = ctx.ReadValue<Vector2>();	
			rightStickText.text = look.ToString();
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string axis)
		{
			if (axis == "Horizontal")
			{
				return look.x;
			}

			if (axis == "Vertical")
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

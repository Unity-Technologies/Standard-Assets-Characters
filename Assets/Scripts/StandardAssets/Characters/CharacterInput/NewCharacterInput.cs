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
		//[SerializeField]
		public GameplayInputActions gamepadControls;
		
		//public MobileInputActions mobileControls;
		
		/*
		 * [SerializeField]
		private GameplayInputActions controls;
		 */
		
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
			gamepadControls.Enable();
			
			gamepadControls.gamepad.movement.performed += Move;
			
			//mobileControls.gamepad.look.performed += Look;
			gamepadControls.gamepad.jump.performed += Jump;
			
			//Keyboard
			
			
			
			//controls.gameplay.dPad.performed += Move;
			foreach (InputActionReference inputActionReference in lookActionReferences)
			{
				inputActionReference.action.performed += Look;
			}
			
			/*
			 * controls.Enable();
			controls.gameplay.movement.performed += Move;
			//controls.gameplay.look.performed += Look;
			controls.gameplay.jump.performed += Jump;
			 */
			//
			/*
			//
			
			
			
			
			mobileControls.Enable();

			mobileControls.gamePlay.look.performed += Look;
			mobileControls.gamePlay.movement.performed += Move;
*/
			
		

			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		public void OnDisable()
		{
			
			gamepadControls.Disable();
			gamepadControls.gamepad.movement.performed -= Move;
		//	mobileControls.keyboardMouse.dPadMovement.performed -= Move;
		//	mobileControls.gamepad.look.performed -= Look;
			gamepadControls.gamepad.jump.performed -= Jump;
		//	controls.gameplay.dPad.performed -= Move;
			
			//Keyboard 
			

			
			foreach (InputActionReference inputActionReference in lookActionReferences)
			{
				inputActionReference.action.performed -= Look;
			}
			
			/*
			 * controls.Enable();
			controls.gameplay.movement.performed -= Move;
			//controls.gameplay.look.performed -= Look;
			controls.gameplay.jump.performed -= Jump;
			//controls.gameplay.jump.performed -= Jump;
			 */

			/*
			 * 
			mobileControls.Disable();
			mobileControls.gamePlay.look.performed -= Look;
			mobileControls.gamePlay.movement.performed -= Move;
			
			
			 */
			
			

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

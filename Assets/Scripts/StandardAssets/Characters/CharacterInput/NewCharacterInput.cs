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
		Vector2 m_MoveInput;

		
		public NewInputActions controls;

		Vector2 m_Look;
		
		Action m_Jump;
		
		public Vector2 moveInput
		{
			get { return m_MoveInput; }
		}

		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}

		public Action jumpPressed
		{
			get { return m_Jump; }
			set { m_Jump = value; }
		}

		
		
		void OnEnable()
		{
			controls.Enable();
			controls.gameplay.movement.performed += Move;
			controls.gameplay.look.performed += Look;
			controls.gameplay.jump.performed += Jump;

			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		void OnDisable()
		{
			controls.Disable();
			controls.gameplay.movement.performed -= Move;
			controls.gameplay.look.performed -= Look;
		}

		void Move(InputAction.CallbackContext ctx)
		{
			m_MoveInput = ctx.ReadValue<Vector2>();
		}

		void Look(InputAction.CallbackContext ctx)
		{
			m_Look = ctx.ReadValue<Vector2>();
			
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		float LookInputOverride(string axis)
		{
			if (axis == "Mouse X")
			{
				return m_Look.x;
			}

			if (axis == "Mouse Y")
			{
				return m_Look.y;
			}

			return 0;
		}

		void Jump(InputAction.CallbackContext ctx)
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}	
		}
	}
	
}

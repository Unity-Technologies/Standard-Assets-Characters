using System;
using System.Security.Cryptography;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
	public class NewCharacterInput : MonoBehaviour, ICharacterInput
	{
		Vector2 m_MoveInput;

		public DemoInputActions controls;

		Vector2 m_Look;
		
		Action m_Jump;
		
		public void OnEnable()
		{
			controls.Enable();
			controls.gameplay.movement.performed += Move;
			controls.gameplay.look.performed += Look;
		}

		public void OnDisable()
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
		
		void Awake()
		{
				
			controls.gameplay.jump.performed += ctx => Jump();
			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		float LookInputOverride(string axis)
		{
			if (axis == "Mouse X")
			{
				//Invert value to match legacy input
				return -m_Look.x;
			}

			if (axis == "Mouse Y")
			{
				return m_Look.y;
			}

			return 0;
		}

		void Jump()
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}	
		}

		public Vector2 moveInput
		{
			get { return m_MoveInput; }
		}

		public bool hasMovementInput
		{
			get { return moveInput.sqrMagnitude > 0; }
		}

		public Action jumpPressed
		{
			get { return m_Jump; }
			set { m_Jump = value; }
		}
	}
	
}

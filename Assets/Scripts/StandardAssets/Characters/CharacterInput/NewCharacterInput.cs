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


		private Vector2 m_look;

		public float rotateSpeed = 10f;
		
		
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
			m_look = ctx.ReadValue<Vector2>();
		}
		
		void Awake()
		{
				
			controls.gameplay.jump.performed += ctx => Jump();
			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		void Update()
		{
			Debug.Log(m_look.ToString());
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		float LookInputOverride(string axis)
		{
			if (axis == "Mouse X")
			{
				return m_look.x;
			}

			if (axis == "Mouse Y")
			{
				return m_look.y;
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

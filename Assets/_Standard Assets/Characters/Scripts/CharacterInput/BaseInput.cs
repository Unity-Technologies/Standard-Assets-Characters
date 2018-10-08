using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Interactions;
using UnityEngine.UI;

namespace StandardAssets.Characters.CharacterInput
{
	public abstract class BaseInput : MonoBehaviour
	{
		[SerializeField]
		protected Controls controls;

		public event Action jumpPressed, sprintStarted, sprintEnded;

		public bool hasMovementInput
		{
			get { return moveInput != Vector2.zero; }
		}

		protected Vector2 moveInputVector;

		protected Vector2 lookInputVector;

		public Vector2 lookInput
		{
			get { return lookInputVector; }
			set { lookInputVector = value; }
		}

		public Vector2 moveInput
		{
			get { return moveInputVector; }
			set { moveInputVector = value; }
		}

		public bool hasJumpInput { get; private set; }

		protected bool isSprinting;

		private void Awake()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
			controls.Movement.move.performed += ctx => moveInputVector = ConditionMoveInput(ctx.ReadValue<Vector2>());
			controls.Movement.look.performed += ctx => lookInputVector = ctx.ReadValue<Vector2>();
			controls.Movement.jump.performed += OnJumpInput;
			controls.Movement.sprint.performed += OnSprintInput;
			RegisterAdditionalInputs();
		}

		protected abstract Vector2 ConditionMoveInput(Vector2 rawMoveInput);
		
		protected abstract void RegisterAdditionalInputs();

		protected virtual void OnSprintInput(InputAction.CallbackContext obj)
		{
			BroadcastInputAction(ref isSprinting, sprintStarted, sprintEnded);
		}

		private void OnEnable()
		{
			controls.Enable();
		}

		private void OnDisable()
		{
			controls.Disable();
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string axis)
		{
			if (axis == "Horizontal")
			{
				return -lookInput.x;
			}

			if (axis == "Vertical")
			{
				return -lookInput.y;
			}

			return 0;
		}

		private void OnJumpInput(InputAction.CallbackContext ctx)
		{
			hasJumpInput = !hasJumpInput;
			if (hasJumpInput)
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
		}

		protected void BroadcastInputAction(ref bool isDoingAction, Action started, Action ended)
		{
			isDoingAction = !isDoingAction;

			if (isDoingAction)
			{
				if (started != null)
				{
					started();
				}
			}
			else
			{
				if (ended != null)
				{
					ended();
				}
			}
		}
	}
}
using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract base class for First Person and Third Person characters
	/// </summary>
	public abstract class CharacterInput : MonoBehaviour
	{
		/// <summary>
		/// Fired when the jump input is pressed - i.e. on key down
		/// </summary>
		public event Action jumpPressed;

		/// <summary>
		/// Fired when the sprint input is started
		/// </summary>
		public event Action sprintStarted;

		/// <summary>
		/// Fired when the sprint input is disengaged
		/// </summary>
		public event Action sprintEnded;

		/// <summary>
		/// The Input Action Map asset
		/// </summary>
		[SerializeField, Tooltip("The Input Action Map asset")]
		protected Controls controls;
		
		/// <summary>
		/// Invert vertical look direction
		/// </summary>
		[SerializeField, Tooltip("Invert vertical look direction")]
		protected bool invertY;
		
		/// <summary>
		/// Invert horizontal look direction
		/// </summary>
		[SerializeField, Tooltip("Invert horizontal look direction")]
		protected bool invertX;

		/// <summary>
		/// The horizontal look sensitivity
		/// </summary>
		[SerializeField, Range(0f, 1f), Tooltip("The horizontal look sensitivity")]
		protected float xSensitivity = 1f;
		
		/// <summary>
		/// The vertical look sensitivity
		/// </summary>
		[SerializeField, Range(0f, 1f), Tooltip("The vertical look sensitivity")]
		protected float ySensitivity = 1f;
		
		/// <summary>
		/// Toggle the cursor lock mode while in play mode.
		/// </summary>
		[SerializeField, Tooltip("Toggle the Cursor Lock Mode, press ESCAPE during play mode")]
		protected bool cursorLocked = true;

		/// <summary>
		/// Gets if the movement input is being applied
		/// </summary>
		public bool hasMovementInput
		{
			get { return moveInput != Vector2.zero; }
		}

		/// <summary>
		/// Gets/sets the look input vector
		/// </summary>
		public Vector2 lookInput { get; protected set; }

		/// <summary>
		/// Gets/sets the move input vector
		/// </summary>
		public Vector2 moveInput { get; protected set; }

		/// <summary>
		/// Gets whether or not the jump input is currently applied
		/// </summary>
		public bool hasJumpInput { get; private set; }

		protected bool isSprinting;

		/// <summary>
		/// Sets up the Cinemachine delegate and subscribes to new input's performed events
		/// </summary>
		protected virtual void Awake()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
			controls.Movement.move.performed += OnMove;
			controls.Movement.look.performed += OnLook;
			controls.Movement.jump.performed += OnJumpInput;
			controls.Movement.sprint.performed += OnSprintInput;
			RegisterAdditionalInputs();
		}

		/// <summary>
		/// Conditions the move input vector
		/// </summary>
		/// <param name="rawMoveInput">The move input vector received from the input action</param>
		/// <returns>A conditioned version of the <paramref name="rawMoveInput"/></returns>
		protected abstract Vector2 ConditionMoveInput(Vector2 rawMoveInput);

		/// <summary>
		/// Handles registration of additional inputs that are not common between the First and Third person characters
		/// </summary>
		protected abstract void RegisterAdditionalInputs();

		/// <summary>
		/// Handles the sprint input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		protected virtual void OnSprintInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref isSprinting, sprintStarted, sprintEnded);
		}

		/// <summary>
		/// Enables associated controls
		/// </summary>
		protected virtual void OnEnable()
		{
			controls.Enable();
			HandleCursorLock();
		}

		/// <summary>
		/// Disables associated controls
		/// </summary>
		protected virtual void OnDisable()
		{
			controls.Disable();
		}

		/// <summary>
		/// Handles the Cinemachine delegate
		/// </summary>
		private float LookInputOverride(string axis)
		{
			if (axis == "Horizontal")
			{
				return invertX ? lookInput.x * xSensitivity : -lookInput.x * xSensitivity;
			}

			if (axis == "Vertical")
			{
				return invertY ? lookInput.y * ySensitivity : -lookInput.y * ySensitivity;
			}

			return 0;
		}

		/// <summary>
		/// Handles the jump event from the new input system
		/// </summary>
		/// <param name="context">Information provided to callback about the jump action.</param>
		private void OnJumpInput(InputAction.CallbackContext context)
		{
			hasJumpInput = !hasJumpInput;
			if (hasJumpInput && jumpPressed != null)
			{
				jumpPressed();
			}
		}
		
		/// <summary>
		/// Assigns <see cref="moveInput"/> based on the move action callback information.
		/// </summary>
		/// <param name="context">Information provided to callback about the move action.</param>
		private void OnMove(InputAction.CallbackContext context)
		{
			moveInput = ConditionMoveInput(context.ReadValue<Vector2>());
		}

		/// <summary>
		/// Assigns <see cref="lookInput"/> based on the look callback information.
		/// </summary>
		/// <param name="context">Information provided to callback about the look action.</param>
		private void OnLook(InputAction.CallbackContext context)
		{
			lookInput = context.ReadValue<Vector2>();
		}
		
		/// <summary>
		/// Handles the cursor lock state
		/// </summary>
		private void HandleCursorLock()
		{
			Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
		
		/// <summary>
		/// Checks for lock state input
		/// </summary>
		protected virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				cursorLocked = !cursorLocked;
				HandleCursorLock();
			}
		}

		/// <summary>
		/// Helper function for broadcasting the start and end events of a specific action. e.g. start sprint and end sprint
		/// </summary>
		/// <param name="isDoingAction">The boolean to toggle</param>
		/// <param name="started">The start event</param>
		/// <param name="ended">The end event</param>
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
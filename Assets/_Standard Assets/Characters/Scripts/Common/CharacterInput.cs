#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
#define TOUCH_CONTROLS
#endif

using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Serialization;

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
		[SerializeField, Tooltip("Input Action Map asset for touch controls")]
		TouchControls m_TouchControls;

		/// <summary>
		/// The Input Action Map asset for on screen controls
		/// </summary>
		[SerializeField, Tooltip("Input Action Map asset for mouse/keyboard and game pad inputs")]
		StandardControls m_StandardControls;

		/// <summary>
		/// The on screen controls canvas
		/// </summary>
		[SerializeField, Tooltip("Canvas used to render the on screen touch control graphics")]
		GameObject m_TouchControlsCanvas;

		/// <summary>
		/// Invert horizontal look direction
		/// </summary>
		[SerializeField, Tooltip("Invert horizontal look direction?")]
		bool m_InvertX;
		
		/// <summary>
		/// Invert vertical look direction
		/// </summary>
		[SerializeField, Tooltip("Invert vertical look direction?")]
		bool m_InvertY;

		/// <summary>
		/// The horizontal look sensitivity
		/// </summary>
		[SerializeField, Range(0f, 1f), Tooltip("Horizontal look sensitivity")]
		float m_XSensitivity = 1f;

		/// <summary>
		/// The vertical look sensitivity
		/// </summary>
		[SerializeField, Range(0f, 1f), Tooltip("Vertical look sensitivity")]
		float m_YSensitivity = 1f;

		/// <summary>
		/// Toggle the cursor lock mode while in play mode.
		/// </summary>
		[SerializeField, Tooltip("Toggle the Cursor Lock Mode? Press ESCAPE during play mode to unlock")]
		bool m_CursorLocked = true;

		bool m_IsSprinting;

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
		public Vector2 lookInput { get; private set; }

		/// <summary>
		/// Gets/sets the move input vector
		/// </summary>
		public Vector2 moveInput { get; private set; }

		/// <summary>
		/// Gets whether or not the jump input is currently applied
		/// </summary>
		public bool hasJumpInput { get; private set; }
		
		protected TouchControls touchControls
		{
			get { return m_TouchControls; }
		}

		protected StandardControls standardControls
		{
			get { return m_StandardControls; }
		}
		
		protected bool isSprinting
		{
			get { return m_IsSprinting; }
			set { m_IsSprinting = value; }
		}

		/// <summary>
		/// Sets up the Cinemachine delegate and subscribes to new input's performed events
		/// </summary>
		void Awake()
		{
			hasJumpInput = false;
			CinemachineCore.GetInputAxis = LookInputOverride;

#if TOUCH_CONTROLS
			m_CursorLocked = false;
			HandleCursorLock();
			if (m_TouchControls != null)
			{
				m_TouchControls.Movement.move.performed +=OnMoveInput;
				m_TouchControls.Movement.look.performed += OnLookInput;
				m_TouchControls.Movement.jump.performed += OnJumpInputEnded;
				m_TouchControls.Movement.jump.started += OnJumpInputStarted;
				m_TouchControls.Movement.sprint.performed += OnSprintInput;

				RegisterAdditionalTouchInputs();
			}

			ToggleTouchControlsCanvas(true);
#else
			if(m_StandardControls !=null)
			{
				m_StandardControls.Movement.move.performed +=OnMoveInput;
				m_StandardControls.Movement.look.performed += OnLookInput;
				m_StandardControls.Movement.sprint.performed += OnSprintInput;
				m_StandardControls.Movement.jump.performed += OnJumpInputEnded;
				m_StandardControls.Movement.jump.started += OnJumpInputStarted;
			
				RegisterAdditionalInputs();
			}
			
			ToggleTouchControlsCanvas(false);	
#endif
		}

		void OnLookInput(InputAction.CallbackContext context)
		{
			lookInput = context.ReadValue<Vector2>();
		}

		void OnMoveInput(InputAction.CallbackContext context)
		{
			moveInput = context.ReadValue<Vector2>();
		}

		/// <summary>
		/// Handles registration of additional inputs that are not common between the First and Third person characters
		/// </summary>
		protected abstract void RegisterAdditionalInputs();

		/// <summary>
		/// Handles registration of additional on screen inputs that are not common between the First and Third person characters 
		/// </summary>
		protected abstract void RegisterAdditionalTouchInputs();

		/// <summary>
		/// Toggle the onscreen controls canvas 
		/// </summary>
		/// <param name="active">canvas game object on or off</param>
		void ToggleTouchControlsCanvas(bool active)
		{
			if (m_TouchControlsCanvas != null)
			{
				m_TouchControlsCanvas.SetActive(active);
			}
		}

		/// <summary>
		/// Handles the sprint input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		protected virtual void OnSprintInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsSprinting, sprintStarted, sprintEnded);
		}

		/// <summary>
		/// Enables associated controls
		/// </summary>
		protected virtual void OnEnable()
		{
#if TOUCH_CONTROLS
			m_TouchControls.Enable();
#else
			m_StandardControls.Enable();
			#endif
			HandleCursorLock();
		}

		/// <summary>
		/// Disables associated controls
		/// </summary>
		protected virtual void OnDisable()
		{
#if TOUCH_CONTROLS
			m_TouchControls.Disable();
#else
			m_StandardControls.Disable();
			#endif
		}

		/// <summary>
		/// Handles the Cinemachine delegate
		/// </summary>
		float LookInputOverride(string axis)
		{
			if (axis == "Horizontal")
			{
				return m_InvertX ? lookInput.x * m_XSensitivity : -lookInput.x * m_XSensitivity;
			}

			if (axis == "Vertical")
			{
				return m_InvertY ? lookInput.y * m_YSensitivity : -lookInput.y * m_YSensitivity;
			}

			return 0;
		}

		/// <summary>
		/// Handles the jump event from the new input system
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		void OnJumpInputEnded(InputAction.CallbackContext context)
		{
			hasJumpInput = false;
		}
		
		void OnJumpInputStarted(InputAction.CallbackContext context)
		{
			hasJumpInput = true;
			if (jumpPressed != null)
			{
				jumpPressed();
			}
		}

		/// <summary>
		/// Handles the cursor lock state
		/// </summary>
		void HandleCursorLock()
		{
			Cursor.lockState = m_CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}

		/// <summary>
		/// Checks for lock state input
		/// </summary>
		protected virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				m_CursorLocked = !m_CursorLocked;
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
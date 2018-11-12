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
		[FormerlySerializedAs("mobileControls")]
		[SerializeField, Tooltip("Input Action Map asset for on screen controls")]
		ControlsMobile m_MobileControls;

		/// <summary>
		/// The Input Action Map asset for on screen controls
		/// </summary>
		[FormerlySerializedAs("controls")]
		[SerializeField, Tooltip("Input Action Map asset for mouse/keyboard and controller")]
		Controls m_Controls;

		/// <summary>
		/// The on screen controls canvas
		/// </summary>
		[FormerlySerializedAs("onScreenControlsCanvas")]
		[SerializeField, Tooltip("Canvas for the onscreen controls")]
		GameObject m_OnScreenControlsCanvas;

		/// <summary>
		/// Invert horizontal look direction
		/// </summary>
		[FormerlySerializedAs("invertX")]
		[SerializeField, Tooltip("Invert horizontal look direction?")]
		bool m_InvertX;
		
		/// <summary>
		/// Invert vertical look direction
		/// </summary>
		[FormerlySerializedAs("invertY")]
		[SerializeField, Tooltip("Invert vertical look direction?")]
		bool m_InvertY;

		/// <summary>
		/// The horizontal look sensitivity
		/// </summary>
		[FormerlySerializedAs("xSensitivity")]
		[SerializeField, Range(0f, 1f), Tooltip("Horizontal look sensitivity")]
		float m_XSensitivity = 1f;

		/// <summary>
		/// The vertical look sensitivity
		/// </summary>
		[FormerlySerializedAs("ySensitivity")]
		[SerializeField, Range(0f, 1f), Tooltip("Vertical look sensitivity")]
		float m_YSensitivity = 1f;

		/// <summary>
		/// Toggle the cursor lock mode while in play mode.
		/// </summary>
		[FormerlySerializedAs("cursorLocked")]
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
		public Vector2 lookInput { get; protected set; }

		/// <summary>
		/// Gets/sets the move input vector
		/// </summary>
		public Vector2 moveInput { get; protected set; }

		/// <summary>
		/// Gets whether or not the jump input is currently applied
		/// </summary>
		public bool hasJumpInput { get; private set; }
		
		protected ControlsMobile mobileControls
		{
			get { return m_MobileControls; }
		}

		protected Controls controls
		{
			get { return m_Controls; }
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
			if (m_MobileControls != null)
			{
				m_MobileControls.Movement.move.performed +=OnMoveInput;
				m_MobileControls.Movement.look.performed += OnLookInput;
				m_MobileControls.Movement.jump.performed += OnJumpInputEnded;
				m_MobileControls.Movement.jump.started += OnJumpInputStarted;
				m_MobileControls.Movement.sprint.performed += OnSprintInput;

				RegisterAdditionalInputsMobile();
			}

			ToggleOnScreenCanvas(true);
#else
			if(m_Controls !=null)
			{
				m_Controls.Movement.move.performed +=OnMoveInput;
				m_Controls.Movement.look.performed += OnLookInput;
				m_Controls.Movement.sprint.performed += OnSprintInput;
				m_Controls.Movement.jump.performed += OnJumpInputEnded;
				m_Controls.Movement.jump.started += OnJumpInputStarted;
			
				RegisterAdditionalInputs();
			}
			
			ToggleOnScreenCanvas(false);	
#endif
		}

		void OnLookInput(InputAction.CallbackContext context)
		{
			lookInput = context.ReadValue<Vector2>();
		}

		void OnMoveInput(InputAction.CallbackContext context)
		{
			moveInput = ConditionMoveInput(context.ReadValue<Vector2>());
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
		/// Handles registration of additional on screen inputs that are not common between the First and Third person characters 
		/// </summary>
		protected abstract void RegisterAdditionalInputsMobile();

		/// <summary>
		/// Toggle the onscreen controls canvas 
		/// </summary>
		/// <param name="active">canvas game object on or off</param>
		void ToggleOnScreenCanvas(bool active)
		{
			if (m_OnScreenControlsCanvas != null)
			{
				m_OnScreenControlsCanvas.SetActive(active);
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
			m_MobileControls.Enable();
#else
			m_Controls.Enable();
			#endif
			HandleCursorLock();
		}

		/// <summary>
		/// Disables associated controls
		/// </summary>
		protected virtual void OnDisable()
		{
#if TOUCH_CONTROLS
			m_MobileControls.Disable();
#else
			m_Controls.Disable();
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
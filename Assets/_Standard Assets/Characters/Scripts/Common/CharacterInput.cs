using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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

		[SerializeField, Tooltip("Input Action Map asset for mouse/keyboard and game pad inputs")]
		StandardControls m_StandardControls;

		[SerializeField, Tooltip("Prefab of canvas used to render the on screen touch control graphics")]
		GameObject m_TouchControlsPrefab;

		[SerializeField, Tooltip("Invert horizontal look direction?")]
		bool m_InvertX;
		
		[SerializeField, Tooltip("Invert vertical look direction?")]
		bool m_InvertY;

		[SerializeField, Range(0f, 1f), Tooltip("Horizontal look sensitivity")]
		float m_XSensitivity = 1f;

		[SerializeField, Range(0f, 1f), Tooltip("Vertical look sensitivity")]
		float m_YSensitivity = 1f;

		[SerializeField, Tooltip("Toggle the Cursor Lock Mode? Press ESCAPE during play mode to unlock")]
		bool m_CursorLocked = true;

		// Instance of UI for Touch Controls
		GameObject m_TouchControlsCanvasInstance;
		
		// Is the character sprinting
		bool m_IsSprinting;

		/// <summary>
		/// Gets if the movement input is being applied
		/// </summary>
		public bool hasMovementInput { get { return moveInput != Vector2.zero; } }

		/// <summary>
		/// Gets/sets the look input vector
		/// </summary>
		public Vector2 lookInput { get; private set; }
		
		/// <summary>
		/// Rotation of a moving platform applied to the look input vector (so that the platform rotates the camera)
		/// </summary>
		public Vector2 movingPlatformLookInput { get; set; }
		
		/// <summary>
		/// Raw look vector. 
		/// </summary>
		private Vector2 rawLookInput;

		/// <summary>
		/// Gets/sets the move input vector
		/// </summary>
		public Vector2 moveInput { get; private set; }

		/// <summary>
		/// Gets whether or not the jump input is currently applied
		/// </summary>
		public bool hasJumpInput { get; private set; }

        /// <summary>
        /// Gets a reference to the currently set Standard Controls asset
        /// </summary>
		protected StandardControls standardControls { get { return m_StandardControls; } }

        /// <summary>
        /// Gets/sets the internal flag that tracks the Sprinting state
        /// </summary>
		protected bool isSprinting
		{
			get { return m_IsSprinting; }
			set { m_IsSprinting = value; }
		}

		// Sets up the Cinemachine delegate and subscribes to new input's performed events
		void Awake()
		{
			hasJumpInput = false;
			
			m_StandardControls = new StandardControls();
			
			if(standardControls != null)
			{
				standardControls.Movement.move.performed += OnMoveInput;
				standardControls.Movement.look.performed += OnLookInput;
				standardControls.Movement.jump.started += OnJumpInputStarted;
				standardControls.Movement.sprint.performed += OnSprintInput;
				
				standardControls.Movement.move.canceled += OnMoveInputCancelled;
				standardControls.Movement.look.canceled += OnLookInputCancelled;
				standardControls.Movement.sprint.canceled += OnSprintInput;
				standardControls.Movement.jump.canceled += OnJumpInputEnded;

				RegisterAdditionalInputs();
			}

			//Handle Touch/OnScreen versus Standard controls
			if(UseTouchControls())
			{
				m_CursorLocked = false;
				HandleCursorLock();
				ToggleTouchControlsCanvas(true);
			}	
			else
			{
				ToggleTouchControlsCanvas(false);	
			}	
		}

		/// <summary>
		/// Enables associated controls
		/// </summary>
		protected void OnEnable()
		{
			standardControls.Enable();
			HandleCursorLock();
			CinemachineCore.GetInputAxis += LookInputOverride;
		}

		/// <summary>
		/// Disables associated controls
		/// </summary>
		protected void OnDisable()
		{
			standardControls.Disable();
			CinemachineCore.GetInputAxis -= LookInputOverride;
		}

		/// <summary>
		/// Checks for lock state input
		/// </summary>
		protected void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				m_CursorLocked = !m_CursorLocked;
				HandleCursorLock();
			}
			
		}	

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
			BroadcastInputAction(ref m_IsSprinting, sprintStarted, sprintEnded);
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

		/// <summary>
		/// Can be called to determine whether or not Touch Controls are being used (instead of Standard Controls)
		/// </summary>
		protected bool UseTouchControls()
		{
			//Assume Touch Controls are wanted for iOS and Android builds
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			return true;
#else
			return false;
#endif
		}

		// Provides the input vector for the look control
		void OnLookInput(InputAction.CallbackContext context)
		{
			lookInput = context.ReadValue<Vector2>();
		}

		// Provides the input vector for the move control
		void OnMoveInput(InputAction.CallbackContext context)
		{
			moveInput = context.ReadValue<Vector2>();
		}
		
		// Resets the move input vector to zero once input has stopped
		void OnMoveInputCancelled(InputAction.CallbackContext context)
		{
			moveInput = Vector2.zero;
		}
			
		// Resets the look input vector to zero once input has stopped
		void OnLookInputCancelled(InputAction.CallbackContext context)
		{
			lookInput = Vector2.zero;
		}

		// Handles the ending of jump event from the new input system
		void OnJumpInputEnded(InputAction.CallbackContext context)
		{
			hasJumpInput = false;
		}
		
		// Handles the start of the jump event from the new input system
		void OnJumpInputStarted(InputAction.CallbackContext context)
		{
			hasJumpInput = true;
			if (jumpPressed != null)
			{
				jumpPressed();
			}
		}

		// Initializes the Touch Controls when need
		void ToggleTouchControlsCanvas(bool active)
		{
			if (m_TouchControlsCanvasInstance != null)
			{
				m_TouchControlsCanvasInstance.SetActive(active);
			}

			if (active && m_TouchControlsCanvasInstance == null)
			{
				m_TouchControlsCanvasInstance = Instantiate(m_TouchControlsPrefab);
				m_TouchControlsCanvasInstance.SetActive(true);
			}
		}		

		// Handles the Cinemachine delegate
		float LookInputOverride(string axis)
		{
			if (axis == "Horizontal")
			{
				return m_InvertX
					? lookInput.x * m_XSensitivity + movingPlatformLookInput.x
					: -lookInput.x * m_XSensitivity + movingPlatformLookInput.x;
			}

			if (axis == "Vertical")
			{
				return m_InvertY ? lookInput.y * m_YSensitivity : -lookInput.y * m_YSensitivity;
			}

			return 0;
		}

		// Handles the cursor lock state
		void HandleCursorLock()
		{
			Cursor.lockState = m_CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
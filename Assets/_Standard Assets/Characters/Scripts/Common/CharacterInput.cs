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
		
		/// <summary>
		/// Look input axis sensitivity
		/// </summary>
		[Serializable]
		public struct Sensitivity
		{	
			[SerializeField, Range(0.01f, 3f), Tooltip("Look sensitivity for mouse")]
			float m_MouseVertical;
			[SerializeField, Range(0.01f, 3f), Tooltip("Look sensitivity for mouse")] 
			float m_MouseHorizontal;
						
			[SerializeField, Range(0.01f, 3f), Tooltip("Look sensitivity for analog gamepad stick")]
			float m_GamepadVertical;
			[SerializeField, Range(0.01f, 3f), Tooltip("Look sensitivity for analog gamepad stick")]
			float m_GamepadHorizontal;
			
			[SerializeField, Range(0.01f, 3f), Tooltip("Look sensitivity for on screen touch stick")]
			float m_TouchVertical;
			[SerializeField, Range(0.01f, 3f), Tooltip("Look sensitivity for on screen touch stick")]
			float m_TouchHorizontal;

			public float mouseVerticalSensitivity { get { return m_MouseVertical; } }
			
			public float mouseHorizontalSensitivity { get { return m_MouseHorizontal; } }
			
			public float gamepadVerticalSensitivity { get { return m_GamepadVertical; } }
			
			public float gamepadHorizontalSensitivity { get { return m_GamepadHorizontal; } }
			
			public float touchVerticalSensitivity { get { return m_TouchVertical; } }
			
			public float touchHorizontalSensitivity { get { return m_TouchHorizontal; } }
		}

		[SerializeField, Tooltip("Input Action Map asset for mouse/keyboard and game pad inputs")]
		StandardControls m_StandardControls;

		[SerializeField, Tooltip("Prefab of canvas used to render the on screen touch control graphics")]
		GameObject m_TouchControlsPrefab;

		[SerializeField, Tooltip("Invert horizontal look direction?")]
		bool m_InvertX;
		
		[SerializeField, Tooltip("Invert vertical look direction?")]
		bool m_InvertY;
		
		[SerializeField, Tooltip("Vertical and Horizontal axis sensitivity")]
		Sensitivity m_CameraLookSensitivity; 

		[SerializeField, Tooltip("Toggle the Cursor Lock Mode? Press ESCAPE during play mode to unlock")]
		bool m_CursorLocked = true;

		// Instance of UI for Touch Controls
		GameObject m_TouchControlsCanvasInstance;
		
		// Is the character sprinting
		bool m_IsSprinting;
		
		// Was the last look input from a mouse
		bool m_UsingMouseInput;
		
		// Check if look input was processed
		bool m_MouseLookInputHasProcessed;
		
		// The frame count when an input axis was processed 
		int m_LookInputProcessedFrame;
		
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
		
		void Awake()
		{			
			hasJumpInput = false;
			
			m_StandardControls = new StandardControls();	
			
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
		/// Sets up the Cinemachine delegate.
		/// Enables associated controls and subscribes to new input's performed events.
		/// </summary>
		protected void OnEnable()
		{
			CinemachineCore.GetInputAxis += LookInputOverride;

			if (standardControls != null)
			{
				standardControls.Movement.move.performed += OnMoveInput;
				standardControls.Movement.mouseLook.performed += OnMouseLookInput;
				standardControls.Movement.gamepadLook.performed += OnGamepadLookInput;
				standardControls.Movement.jump.started += OnJumpInputStarted;
				standardControls.Movement.sprint.performed += OnSprintInput;
				 
				standardControls.Movement.move.canceled += OnMoveInputCanceled;
				standardControls.Movement.sprint.canceled += OnSprintInput;
				standardControls.Movement.jump.canceled += OnJumpInputEnded;
				standardControls.Movement.gamepadLook.canceled += OnLookInputCanceled;
				
				RegisterAdditionalInputs();
				
				standardControls.Enable();
			}
			
			HandleCursorLock();
		}

		/// <summary>
		/// Disables the Cinemachine delegate.
		/// Disables associated controls and unsubscribes from new input's performed events.
		/// </summary>
		protected void OnDisable()
		{
			CinemachineCore.GetInputAxis -= LookInputOverride;

			if (standardControls != null)
			{
				standardControls.Movement.move.performed -= OnMoveInput;
				standardControls.Movement.mouseLook.performed -= OnMouseLookInput;
				standardControls.Movement.gamepadLook.performed -= OnGamepadLookInput;
				standardControls.Movement.jump.started -= OnJumpInputStarted;
				standardControls.Movement.sprint.performed += OnSprintInput;
				 
				standardControls.Movement.move.canceled -= OnMoveInputCanceled;
				standardControls.Movement.sprint.canceled -= OnSprintInput;
				standardControls.Movement.jump.canceled -= OnJumpInputEnded;
				standardControls.Movement.gamepadLook.canceled -= OnLookInputCanceled;

				DeRegisterAdditionalInputs();
			
				standardControls.Disable();
			}
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
		/// Handles registration of additional inputs that are not common between the First and Third person characters
		/// </summary>
		protected abstract void DeRegisterAdditionalInputs();

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

		// Provides the input vector for the mouse look control.
		// If the mouse look input was already processed, then clear the value before accumulating again. 
		void OnMouseLookInput(InputAction.CallbackContext context)
		{
			var newInput = context.ReadValue<Vector2>();
			m_UsingMouseInput = true;
			
			if (m_MouseLookInputHasProcessed)
			{
				lookInput = Vector2.zero;
				m_MouseLookInputHasProcessed = false;
			}
			
			lookInput += newInput;		
		}

		// Provides the input vector for the gamepad look control
		void OnGamepadLookInput(InputAction.CallbackContext context)
		{
			m_UsingMouseInput = false;
			lookInput = context.ReadValue<Vector2>();
		}
		
		// Provides the input vector for the move control
		void OnMoveInput(InputAction.CallbackContext context)
		{
			moveInput = context.ReadValue<Vector2>();
		}
		
		// Resets the move input vector to zero once input has stopped
		void OnMoveInputCanceled(InputAction.CallbackContext context)
		{
			moveInput = Vector2.zero;
		}
		
		// Resets the look input vector to zero once it has stopped. This is only used for analogue stick input
		void OnLookInputCanceled(InputAction.CallbackContext context)
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
			// Handle the clearing of mouse look inputs 
			if (m_UsingMouseInput)
			{
				ProcessMouseInput();
			}
			
			if (axis == "Vertical")
			{	
				var lookVertical = m_InvertY ? lookInput.y : -lookInput.y;
				if (UseTouchControls())
				{
					lookVertical *= m_CameraLookSensitivity.touchVerticalSensitivity;
				}
				else
				{
					lookVertical *= m_UsingMouseInput
						? m_CameraLookSensitivity.mouseVerticalSensitivity
						: m_CameraLookSensitivity.gamepadVerticalSensitivity;
				}
				return lookVertical;
			}

			if (axis == "Horizontal")
			{
				var lookHorizontal = m_InvertX
					? lookInput.x + movingPlatformLookInput.x
					: -lookInput.x + movingPlatformLookInput.x;
				if (UseTouchControls())
				{
					lookHorizontal *= m_CameraLookSensitivity.touchHorizontalSensitivity;
				}
				else
				{
					lookHorizontal *= m_UsingMouseInput 
						? m_CameraLookSensitivity.mouseHorizontalSensitivity 
						: m_CameraLookSensitivity.gamepadHorizontalSensitivity;
				}
				return lookHorizontal;
			}
			return 0;
		}
		
		// Called at the beginning of LookInputOverride when using mouse input.
		// This is to ensure that mouse look inputs are properly cleared once they have been processed as mouse
		// input has no canceled action event subscribed to it, and can be set more than once per frame.
		void ProcessMouseInput()
		{
			var currentFrame = Time.frameCount;
			if ((m_LookInputProcessedFrame < currentFrame) && m_MouseLookInputHasProcessed)
			{
				lookInput = Vector2.zero;
			}
			m_LookInputProcessedFrame = currentFrame;
			m_MouseLookInputHasProcessed = true;
		}

		// Handles the cursor lock state
		void HandleCursorLock()
		{
			Cursor.lockState = m_CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
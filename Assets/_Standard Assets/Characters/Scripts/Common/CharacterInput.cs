using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract base class for First Person and Third Person characters
	/// </summary>
	public abstract class CharacterInput : MonoBehaviour, StandardControls.IMovementActions
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
			[SerializeField, Range(0.2f, 2f), Tooltip("Look sensitivity for mouse")]
			float m_MouseVertical;
			[SerializeField, Range(0.2f, 2f), Tooltip("Look sensitivity for mouse")] 
			float m_MouseHorizontal;
						
			[SerializeField, Range(0.2f, 2f), Tooltip("Look sensitivity for analog gamepad stick")]
			float m_GamepadVertical;
			[SerializeField, Range(0.2f, 2f), Tooltip("Look sensitivity for analog gamepad stick")]
			float m_GamepadHorizontal;
			
			[SerializeField, Range(0.2f, 2f), Tooltip("Look sensitivity for on screen touch stick")]
			float m_TouchVertical;
			[SerializeField, Range(0.2f, 2f), Tooltip("Look sensitivity for on screen touch stick")]
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
		
		[SerializeField, Tooltip("Vertical and Horizontal axis sensitivity")]
		Sensitivity m_CameraLookSensitivity; 

		[SerializeField, Tooltip("Toggle the Cursor Lock Mode? Press ESCAPE during play mode to unlock")]
		bool m_CursorLocked = true;
		
		//Component used to allow for more precise mouse look as well as dampened gamepad look
		CinemachineInputGainDampener m_CameraInputGainAcceleration;

		// Instance of UI for Touch Controls
		GameObject m_TouchControlsCanvasInstance;
		
		// Is the character sprinting
		bool m_IsSprinting;
		
		// Was the last look input from a mouse
		bool m_UsingMouseInput;
		
		// Check if look input was processed
		bool m_HasProcessedMouseLookInput;
		
		// If there is a not Camera CinemachineInputGainDampener component on the character,
		// then the camera will be controlled from the CharacterInput script.
		bool m_UseInputGainAcceleration;
		
		// The frame count when an input axis was processed 
		int m_LookInputProcessedFrame;
		
		// Look input vector
		Vector2 m_lookInput;
		
		/// <summary>
		/// Gets if the movement input is being applied
		/// </summary>
		public bool hasMovementInput { get { return moveInput != Vector2.zero; } }
		
		/// <summary>
		/// Used to check if the last look input came from a mouse
		/// </summary>
		public bool usingMouseInput
		{
			get { return m_UsingMouseInput;}
		}
		
		/// <summary>
		/// The camera look sensitivity
		/// </summary>
		public Sensitivity cameraLookSensitivity
		{
			get { return m_CameraLookSensitivity; }
		}
		
		/// <summary>
		/// Invert the X axis input
		/// </summary>
		[Tooltip("Invert horizontal look direction?")]
		public bool m_InvertX;
		
		/// <summary>
		/// Invert the Y axis input
		/// </summary>
		[Tooltip("Invert vertical look direction?")]
		public bool m_InvertY;

		/// <summary>
		/// Gets/sets the look input vector
		/// </summary>
		public Vector2 lookInput
		{
			get { return m_lookInput; }
			set { m_lookInput = value; }
		}
		
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
			
			m_CameraInputGainAcceleration = GetComponent<CinemachineInputGainDampener>();

			// If no CinemachineInputGainDampener component is present, then there will be no acceleration or deceleration 
			// applied to the look input. If that is the case, it is advised to set the Speed Mode of the Cinemachine cameras
			// to 'Max Speed' if it is desirable to have acceleration and deceleration for gamepad look input. 
			if (m_CameraInputGainAcceleration != null)
			{
				m_UseInputGainAcceleration = true;
			}
			else
			{
				Debug.LogWarning("No CinemachineInputGainDampener component detected, make sure Cinemachine axis control speed mode is set to 'Max Speed' or add a CameraInputGainAcceleration component to this character");
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
		/// Sets up the Cinemachine delegate.
		/// Enables associated controls and subscribes to new input's performed events.
		/// </summary>
		protected void OnEnable()
		{
			//Do not register the Cinemachine LookInputOverride when using a CinemachineInputGainDampener component
			//This LookInputOverride will take place in CinemachineInputGainDampner
			if (!m_UseInputGainAcceleration)
			{
				CinemachineCore.GetInputAxis += LookInputOverride;
			}

			if (m_StandardControls == null)
			{
				m_StandardControls = new StandardControls();
				m_StandardControls.Movement.SetCallbacks(this);
			}
			m_StandardControls.Movement.Enable();
			
			HandleCursorLock();
		}

		/// <summary>
		/// Disables the Cinemachine delegate.
		/// Disables associated controls and unsubscribes from new input's performed events.
		/// </summary>
		protected void OnDisable()
		{
			if (!m_UseInputGainAcceleration)
			{
				CinemachineCore.GetInputAxis -= LookInputOverride;
			}
			
			m_StandardControls.Disable();
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
		/// Handles the sprint input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public virtual void OnSprint(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsSprinting, sprintStarted, sprintEnded);
		}
		
		/// <summary>
		/// Handles the recentre input. 
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public virtual void OnRecentre(InputAction.CallbackContext context)
		{
			//This implementation is done in ThirdPersonInput
		}

		/// <summary>
		/// Handles the strafe input.
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public virtual void OnStrafe(InputAction.CallbackContext context)
		{
			//This implementation is done in ThirdPersonInput
		}
		
		/// <summary>
		/// Handles the crouch input. 
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public virtual void OnCrouch(InputAction.CallbackContext context)
		{
			//This implementation is done in FirstPersonInput
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
		public bool UseTouchControls()
		{
			//Assume Touch Controls are wanted for iOS and Android builds
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			return true;
#else
			return false;
#endif
		}

		/// <summary>
		/// Provides the input vector for the mouse look control.
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public void OnMouseLook(InputAction.CallbackContext context)
		{
			var newInput = context.ReadValue<Vector2>();
			m_UsingMouseInput = true;
			
			//When using an InputGainAcceleration component, the look input processing should happen there.
			if (m_UseInputGainAcceleration)
			{
				if (m_CameraInputGainAcceleration.hasProcessedMouseLookInput)
				{
					m_lookInput = Vector2.zero;
					m_CameraInputGainAcceleration.hasProcessedMouseLookInput = false;
				}
			}
			else
			{
				// If the mouse look input was already processed, then clear the value before accumulating again
				if (m_HasProcessedMouseLookInput)
				{
					m_lookInput = Vector2.zero;
					m_HasProcessedMouseLookInput = false;
				}
			}
			
			m_lookInput += newInput;			
		}
		
		/// <summary>
		/// Provides the input vector for the gamepad look control.
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public void OnGamepadLook(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				m_UsingMouseInput = false;
				m_lookInput = context.ReadValue<Vector2>();
			}
			else if (context.canceled)
			{
				m_lookInput = Vector2.zero;
			}
		}
		
		/// <summary>
		/// Provides the input vector for the move control.
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public void OnMove(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				moveInput = context.ReadValue<Vector2>();
			}
			else if (context.canceled)
			{
				moveInput = Vector2.zero;
			}
		}
		
		/// <summary>
		/// Handles the jump event from the new input system.
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		public void OnJump(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				hasJumpInput = true;
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
			else if (context.canceled)
			{
				hasJumpInput = false;
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
			// This is to ensure that mouse look inputs are properly cleared once they have been processed as mouse
			// input has no canceled action event subscribed to it, and can be set more than once per frame
			if (m_UsingMouseInput)
			{
				var currentFrame = Time.frameCount;
				if ((m_LookInputProcessedFrame < currentFrame) && m_HasProcessedMouseLookInput)
				{
					m_lookInput = Vector2.zero;
				}
				m_LookInputProcessedFrame = currentFrame;
				m_HasProcessedMouseLookInput = true;
			}
			
			if (axis == "Vertical")
			{	
				var lookVertical = m_InvertY ? m_lookInput.y : -m_lookInput.y;
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
					? m_lookInput.x + movingPlatformLookInput.x
					: -m_lookInput.x + movingPlatformLookInput.x;
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
		
		// Handles the cursor lock state
		void HandleCursorLock()
		{
			Cursor.lockState = m_CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
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
		
		[SerializeField, Range(0.01f, 2f), Tooltip("Look sensitivity for mouse")]
		float m_MouseSensitivity = 1f;

		[SerializeField, Range(0.01f, 2f), Tooltip("Look sensitivity for analogue stick")]
		float m_AnalogueStickSensitivity = 1f;

		[SerializeField, Tooltip("Toggle the Cursor Lock Mode? Press ESCAPE during play mode to unlock")]
		bool m_CursorLocked = true;

		// Instance of UI for Touch Controls
		GameObject m_TouchControlsCanvasInstance;
		
		// Is the character sprinting
		bool m_IsSprinting;
		
		// Was the last look input from a mouse
		bool m_UsingMouseInput;
		
		// The frame count when a mouse look input was set
		int m_LookInputSetFrame;
		
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
		
		// Sets up the Cinemachine delegate and subscribes to new input's performed events
		void Awake()
		{			
			hasJumpInput = false;
			
			m_StandardControls = new StandardControls();	
			
			if(standardControls != null)
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
			CinemachineCore.GetInputAxis += LookInputOverride;
			standardControls.Enable();
			HandleCursorLock();
		}

		/// <summary>
		/// Disables associated controls
		/// </summary>
		protected void OnDisable()
		{
			CinemachineCore.GetInputAxis -= LookInputOverride;
			standardControls.Disable();
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

		// Provides the input vector for the mouse look control.
		// If the mouse look input action was already called this frame, then accumulate the lookInput value.
		// Set the frame count when lookInput vector was last set, so that it can be reset once Cinemachine has used it.
		void OnMouseLookInput(InputAction.CallbackContext context)
		{
			var newInput = context.ReadValue<Vector2>();
			m_UsingMouseInput = true;
			
			if (m_LookInputSetFrame == Time.frameCount)
			{
				lookInput += newInput;
			}
			else
			{
				lookInput = newInput;
				m_LookInputSetFrame = Time.frameCount;	
			}
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
		// Keeps track of the last frame when Cinemachine used a look input vector value, if this is after it has been
		// set by the mouse look input, then reset the vector value to 0.
		// This method can be called more than once per frame by Cinemachine when
		// using a Freelook Camera with an Orbital Transposer
		float LookInputOverride(string axis)
		{
			if (axis == "Vertical")
			{	
				var lookVertical = m_InvertY ? lookInput.y : -lookInput.y;
	
				lookVertical *= m_UsingMouseInput ? m_MouseSensitivity : m_AnalogueStickSensitivity;		
				ClearLookInput(1.0f, 0.0f);
				return lookVertical;
			}

			if (axis == "Horizontal")
			{	
				var lookHorizontal = m_InvertX ? lookInput.x + movingPlatformLookInput.x 
					: -lookInput.x + movingPlatformLookInput.x;

				lookHorizontal *= m_UsingMouseInput ? m_MouseSensitivity : m_AnalogueStickSensitivity;
				ClearLookInput(0.0f, 1.0f);
				return lookHorizontal;
			}
			
			return 0;
		}
		
		// Clears the look input vector 
		void ClearLookInput(float xMask, float yMask)
		{
			// If Cinemachine has already used this look input value value after it was set inside of OnMouseLookInput,
			// then it should be reset to zero before being used again. 
			if (m_LookInputProcessedFrame > m_LookInputSetFrame && m_UsingMouseInput)
			{
				// Reset only the masked component of the look vector as Cinemachine may not have consumed it yet.
				lookInput *= new Vector2(xMask, yMask);
			}
			
			m_LookInputProcessedFrame = Time.frameCount;
		}

		// Handles the cursor lock state
		void HandleCursorLock()
		{
			Cursor.lockState = m_CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
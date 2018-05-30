using System;
using Cinemachine;
//using ProBuilder2.Common;
using StandardAssets.Characters.CharacterInput;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.UI;

namespace StandardAssets.Characters.Input
{
    public class MobileCharacterInputTest: MonoBehaviour, ICharacterInput
    {
        Vector2 m_MoveInput;

		private Vector2 moveVector2;

		public Camera mainCamera;

		public MobileControls controls;


		private Vector2 m_look;

		public float rotateSpeed = 10f;
		
		private InputActionManager m_ActionManager;

	    public Image leftControlImage;
	    public Image rightControlImage;

	    public TMP_Text leftTextDebug;
	    public TMP_Text rightTextDebug;
	    public TMP_Text touchDeltaXDebug;
		
	//	public InputAction gamePadLook;

	    private Vector2 leftTouchPos;
	    private Vector2 rightTouchPos;

	    private int screenWidth;
	    private int screenHeight;

	    private Vector2 normalisedDistanceLeft;

	    private Vector2 leftImagePos;
	    private Vector2 adjustedLeftCenter;
	    
	    private Vector2 normalisedDistanceRight;

	    private Vector2 rightImagePos;
	    private Vector2 adjustedRightCenter;

	    private Vector2 touchDelta;
	    
	    void Start()
	    {
		    screenHeight = Screen.height;
		    screenWidth = Screen.width;
		    
		    leftImagePos = leftControlImage.rectTransform.anchoredPosition;
			
		    float adjustedX = (screenWidth/2)+leftImagePos.x;
		    float adjustedY = (screenHeight- ((screenHeight / 2) + leftImagePos.y));
			
		    adjustedLeftCenter = new Vector2(adjustedX,adjustedY);
			
		    //DOuble check this
		    adjustedX = (screenWidth / 2) - rightImagePos.x;
		    adjustedY = screenHeight - ((screenHeight / 2) + rightImagePos.y);
		    adjustedLeftCenter = new Vector2(adjustedX,adjustedY);

		    rightImagePos = rightControlImage.rectTransform.anchoredPosition;
	    }
		
		public void OnEnable()
		{
			controls.Enable();
			//gamePadLook.Enable();
		}

		public void OnDisable()
		{
			controls.Disable();
			//gamePadLook.Disable();
		}
		
		void Awake()
		{
		//	gamePadLook.performed += ctx => GamePadTest();
			//Cinemachine POV axis control override 
			CinemachineCore.GetInputAxis = LookInputOverride;
			//controls.gameplay.look.performed += ctx => m_look = ctx.ReadValue<Vector2>();

			//controls.gameplay.look.performed += ctx => GamePadTest();
			
			//'NEW NEW' Input action manager, this allows a dpad to be set to 
			// WASD
			m_ActionManager = new InputActionManager();

			////TODO: this currently falls over due to missing support for composites in InputActionManager
			////TEMP: we don't yet have support for setting up composite bindings in the UI; hack
			////      in WASD keybindings as a temp workaround
			
			/*
			 controls.gameplay.movement.AppendCompositeBinding("Dpad")
				.With("Left", "<Keyboard>/a")
				.With("Right", "<Keyboard>/d")
				.With("Up", "<Keyboard>/w")
				.With("Down", "<Keyboard>/s");
			*/
			 
			
			
			m_ActionManager.AddActionMap(controls.gameplay);
			//Actions Performed triggers 
			//Gets the 'vector' values for movement and look
			
			//controls.gameplay.movement.performed += ctx => moveVector2 = ctx.ReadValue<Vector2>();
			
			//controls.gameplay.crouch.performed += ctx => m_look = ctx.ReadValue<Vector2>();
			controls.gameplay.lookTouch.performed += ctx => rightTouchPos = ctx.ReadValue<Vector2>();
			controls.gameplay.movementTouch.performed += ctx => leftTouchPos = ctx.ReadValue<Vector2>();

			controls.gameplay.touchDelta.performed += ctx => touchDelta = ctx.ReadValue<Vector2>();

		}

		void GamePadTest()
		{
			Debug.Log("BUTTTON!!!!:");
		}
		
		void Update ()
		{
			
			touchDeltaXDebug.SetText(""+touchDelta);

			if (leftTouchPos.x <= (screenWidth / 2))
			{
				normalisedDistanceLeft = (leftTouchPos-adjustedLeftCenter).normalized;
				leftTextDebug.SetText(normalisedDistanceLeft.ToString());
			}
			
			if (rightTouchPos.x >= (screenWidth / 2))
			{
				normalisedDistanceRight = (rightTouchPos-adjustedRightCenter).normalized;
				rightTextDebug.SetText("IMG: "+rightImagePos+"/n"+rightTouchPos.ToString());
			}
			
			Debug.Log("Left Anchor: "+leftImagePos.ToString());
			Debug.Log("Right : "+rightControlImage.GetPixelAdjustedRect().center.ToString());
			
			//text.SetText("Point: "+mousePos+ " \nLeftImageAnchor: "+leftImagePos.ToString());

			
			 var triggerEvents = m_ActionManager.triggerEventsForCurrentFrame;
			var triggerEventCount = triggerEvents.Count;
	
			for (var i = 0; i < triggerEventCount; ++i)
			{
				var actions = triggerEvents[i].actions;
				var actionCount = actions.Count;
	
				////REVIEW: this is an insanely awkward way of associating actions with responses
				////        the API needs serious work
	
				for (var n = 0; n < actionCount; ++n)
				{
					var action = actions[n].action;
					var phase = actions[n].phase;
	
					
					if (action == controls.gameplay.lookTouch )
					{
						m_look = normalisedDistanceRight;
						//	m_look = triggerEvents[i].ReadValue<Vector2>();
					}
					else if (action == controls.gameplay.movementTouch)
					{
						moveVector2 = normalisedDistanceLeft;
						moveVector2.y *= -1;
					}
					//else if (action == controls.gameplay.mouseLook )
					//{
				//		m_look = triggerEvents[i].ReadValue<Vector2>();
				//	}
					
					
					
				}
			}
			 
			 
			UpdateLook();
			Move();
			
		}

		/// <summary>
		/// Applies the mouse look scale, higher means more "sensitive"
		/// </summary>
		void UpdateLook()
		{
			var sccaledLookSpeed = rotateSpeed * Time.deltaTime;
			m_look *= sccaledLookSpeed;
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

		void Move()
		
		{
			m_MoveInput.Set(moveVector2.x, moveVector2.y);
			moveVector2 = new Vector2(0,0);
			
		}

		

		public Vector2 moveInput
		{
			get { return m_MoveInput; }
		}

	    public bool hasMovementInput { get; private set; }
	    public Action jumpPressed { get; set; }

	    public bool isMoveInput
		{
			get { return moveInput.sqrMagnitude > 0; }
		}

	    public Action jump { get; set; }
    }
}
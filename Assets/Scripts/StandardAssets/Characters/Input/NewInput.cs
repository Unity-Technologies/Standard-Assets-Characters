using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using Cinemachine;
using ProBuilder2.Common;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Input;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Interactions;
using UnityEngine.Experimental.Input.Controls;
using Random = UnityEngine.Random;


namespace StandardAssets.Characters.Input
{
	public class NewInput : MonoBehaviour, IInput
	{
		Vector2 m_MoveInput;

		private Vector2 moveVector2;

		public Camera mainCamera;

		public DemoInputActions controls;


		private Vector2 m_look;

		public float rotateSpeed = 10f;
		
		private InputActionManager m_ActionManager;
		
		Action m_Jump;

	//	public InputAction gamePadLook;
		
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
			
			controls.gameplay.movement.performed += ctx => moveVector2 = ctx.ReadValue<Vector2>();
			
			//controls.gameplay.crouch.performed += ctx => m_look = ctx.ReadValue<Vector2>();
			controls.gameplay.jump.performed += ctx => Jump();

		}

		void GamePadTest()
		{
			Debug.Log("BUTTTON!!!!:");
		}
		
		void Update ()
		{
			
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
	
					
					if (action == controls.gameplay.look )
					{
						m_look = triggerEvents[i].ReadValue<Vector2>();
					}
					else if (action == controls.gameplay.movement)
					{
						moveVector2 = triggerEvents[i].ReadValue<Vector2>();
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
			
		}

		void Jump()
		{
			
			if (jump != null)
			{
				jump();
			}
			
		}


		void Fire(InputAction.CallbackContext context)
		{
			//Fire Action 
		}

		public Vector2 moveInput
		{
			get { return m_MoveInput; }
		}

		public bool isMoveInput
		{
			get { return moveInput.sqrMagnitude > 0; }
		}

		public Action jump
		{
			get { return m_Jump; }
			set { m_Jump = value; }
			
		}
	}
	
}

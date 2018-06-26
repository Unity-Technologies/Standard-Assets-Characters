using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	public class LegacyUnityCharacterInput : MonoBehaviour, ICharacterInput
	{
		[Header("Cinemachine Axes")]
		[SerializeField]
		private string cinemachineLookXAxisName = "Horizontal";
		[SerializeField]
		private string cinemachineLookYAxisName = "Vertical";
		
		[Header("Input Axes")]
		[SerializeField]
		private string horizontalAxisName = "Horizontal";
		
		[SerializeField]
		private string verticalAxisName = "Vertical";

		[SerializeField]
		private bool useLookInput = true;

		[SerializeField]
		private string lookXAxisName = "LookX";
		
		[SerializeField]
		private string lookYAxisName = "LookY";

		[SerializeField]
		private string jumpButtonName = "Jump";

		private Vector2 moveInputVector;
		private Action jumped;

		private Vector2 look;

		public Vector2 moveInput
		{
			get { return moveInputVector; }
		}
		
		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}
		
		public Action jumpPressed
		{
			get { return jumped; }
			set { jumped = value; }
		}

		private void Awake()
		{
			
			//Only works with with XBox One for now
			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			lookXAxisName = "XBoneRightStickXMac";
			lookYAxisName = "XBoneRightStickYMac";
			//jumpButtonName = "XBone Button South MAC";
			#endif
			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			lookXAxisName = "LookX";
			lookYAxisName = "LookY";
			//jumpButtonName = "Jump";
#endif


		}

		private void OnEnable()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		private void Update()
		{
			//Cache the inputs
			moveInputVector.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));

			if (Input.GetButtonDown(jumpButtonName))
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
		
			Debug.Log(Input.GetAxisRaw("XBone rightStick Press Mac"));
			
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string cinemachineAxisName)
		{
			if (!useLookInput)
			{
				return 0;
			}
			
			if (cinemachineAxisName == cinemachineLookXAxisName)
			{
				
				return Input.GetAxis(lookXAxisName);
			}
			if (cinemachineAxisName == cinemachineLookYAxisName)
			{
				
				return Input.GetAxis(lookYAxisName);
			}

			return 0;
		}
	}
}
using System;
using UnityEngine;


namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// ICharacterInput implementation that automatically chooses between Standalone and Mobile inputs based on the platform
	/// </summary>
	/// <seealso cref="LegacyCharacterInput"/>
	/// <seealso cref="LegacyOnScreenCharacterInput"/>
	public class LegacyCrossPlatformCharacterInput : MonoBehaviour, ICharacterInput
	{
		[SerializeField, Tooltip("Input to use for standalone platforms (OSX/Windows/Editor)")]
		protected LegacyCharacterInput standaloneInput;

		[SerializeField, Tooltip("Input to use for mobile platforms (Android/iOS)")]
		protected LegacyOnScreenCharacterInput mobileInput;

		[SerializeField, Tooltip("Allows developers to view and debug the Mobile controls in Editor")]
		protected bool debugOnScreenControls;

		//The current input system being used
		private LegacyCharacterInputBase currentInputSystem;
		
		/// <summary>
		/// The current input system being used
		/// </summary>
		private LegacyCharacterInputBase currentInput
		{
			get
			{
				if (currentInputSystem == null)
				{
					SetControls();
				}

				return currentInputSystem;
			}
		}

		/// <inheritdoc />
		public Vector2 lookInput
		{
			get { return (currentInput != null) ? currentInput.lookInput : Vector2.zero; }
		}

		/// <inheritdoc />
		public Vector2 moveInput
		{
			get { return (currentInput != null) ? currentInput.moveInput : Vector2.zero; }
		}
		
		/// <inheritdoc />
		public bool hasMovementInput
		{
			get { return (currentInput != null) ? currentInput.hasMovementInput : false; }
		}

		/// <inheritdoc />
		public bool hasJumpInput
		{
			get { return (currentInput != null) ? currentInput.hasJumpInput : false; }
		}

		/// <inheritdoc />
		public Action jumpPressed
		{
			get { return (currentInput != null) ? currentInput.jumpPressed : null; }
			set 
			{ 
				if(currentInput != null)
				{
					currentInput.jumpPressed = value; 
				}
			}
		}

		/// <summary>
		/// Sets the current control based on the Platform
		/// </summary>
		private void SetControls()
		{
			if (debugOnScreenControls)
			{
				SetMobileControls();
				return;
			}
			
			#if UNITY_ANDROID || UNITY_IOS
			SetMobileControls();
			#else
			SetStandaloneControls();
			#endif
		}

		/// <summary>
		/// Sets up the mobile controls
		/// </summary>
		private void SetMobileControls()
		{
			Cursor.lockState = CursorLockMode.None;
			currentInputSystem = mobileInput;
			if(mobileInput != null)
			{
				mobileInput.enabled = true;
			}
			if(standaloneInput != null)
			{
				standaloneInput.enabled = false;
			}
		}

		/// <summary>
		/// Sets up the standalone controls
		/// </summary>
		private void SetStandaloneControls()
		{
			currentInputSystem = standaloneInput;
			if(standaloneInput != null)
			{
				standaloneInput.enabled = true;
			}
			if(mobileInput != null)
			{
				mobileInput.enabled = false;
			}
		}
	}
}
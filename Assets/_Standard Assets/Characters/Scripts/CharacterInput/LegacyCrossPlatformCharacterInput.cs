using System;
using UnityEngine;


namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// ICharacterInput implementation that automatically chooses between Standalone and Mobile inputs based on the platform
	/// </summary>
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
			get { return currentInput.lookInput; }
		}

		/// <inheritdoc />
		public Vector2 moveInput
		{
			get { return currentInput.moveInput; }
		}
		
		/// <inheritdoc />
		public bool hasMovementInput
		{
			get { return currentInput.hasMovementInput; }
		}

		/// <inheritdoc />
		public bool hasJumpInput
		{
			get { return currentInput.hasJumpInput; }
		}

		/// <inheritdoc />
		public Action jumpPressed
		{
			get { return currentInput.jumpPressed; }
			set { currentInput.jumpPressed = value; }
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
			currentInputSystem = mobileInput;
			mobileInput.gameObject.SetActive(true);
			standaloneInput.gameObject.SetActive(false);
		}

		/// <summary>
		/// Sets up the standalone controls
		/// </summary>
		private void SetStandaloneControls()
		{
			currentInputSystem = standaloneInput;
			standaloneInput.gameObject.SetActive(true);
			mobileInput.gameObject.SetActive(false);
		}
	}
}
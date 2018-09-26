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
		/// <inheritdoc />
		public event Action jumpPressed;
		
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
					CheckInputsAreSetUp();
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

			if (currentInput != null)
			{
				currentInput.jumpPressed += OnJumpPressed;
			}
		}

		private void OnJumpPressed()
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}
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

		/// <summary>
		/// Ensures inputs are auto-setup if missing
		/// </summary>
		private void CheckInputsAreSetUp()
		{
			if (standaloneInput == null)
			{
				Debug.LogWarning("Standlone Input not set - looking at siblings");
				standaloneInput = GetInputInSibling<LegacyCharacterInput>();
				if (standaloneInput == null)
				{
					Debug.LogWarning("No parent/sibling found - cannot auto-populate with sibling - searching scene");
					standaloneInput = GetInputInScene<LegacyCharacterInput>();
				}
			}
			
			if (mobileInput == null)
			{
				Debug.LogWarning("Mobile Input not set - looking at siblings");
				mobileInput = GetInputInSibling<LegacyOnScreenCharacterInput>();
				if (mobileInput == null)
				{
					Debug.LogWarning("No parent/siblin found - cannot auto-populate with sibling - searching scene");
					if (mobileInput == null)
					{
						mobileInput = GetInputInScene<LegacyOnScreenCharacterInput>();
					}
				}
			}
		}

		/// <summary>
		/// Finds the specified input on sibling
		/// </summary>
		/// <typeparam name="T"><see cref="LegacyCharacterInputBase"/> to look for</typeparam>
		/// <returns><see cref="LegacyCharacterInputBase"/> if found, null otherwise</returns>
		private T GetInputInSibling<T>() where T : LegacyCharacterInputBase
		{
			if (transform.parent != null)
			{
				return transform.parent.GetComponentInChildren<T>();
			}
			
			return null;
		}

		/// <summary>
		/// Searches for specified input in scene
		/// </summary>
		/// <typeparam name="T"><see cref="LegacyCharacterInputBase"/> to look for</typeparam>
		/// <returns><see cref="LegacyCharacterInputBase"/> if found, null otherwise</returns>
		private T GetInputInScene<T>() where T : LegacyCharacterInputBase
		{
			T[] inSceneObjects = FindObjectsOfType<T>();
			if (inSceneObjects.Length == 0)
			{
				Debug.LogError("Cannot find Input in scene");
				gameObject.SetActive(false);
				return null;
			}

			if (inSceneObjects.Length > 1)
			{
				Debug.LogError("Found too many Inputs in scene - be sure to group your 3 character prefabs under one object so that sibling search can be used");
				gameObject.SetActive(false);
				return null;
			}

			return inSceneObjects[0];
		}
	}
}
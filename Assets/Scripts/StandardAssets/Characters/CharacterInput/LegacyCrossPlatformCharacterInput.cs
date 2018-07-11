using System;
using UnityEngine;
using UnityEngine.Networking;

namespace StandardAssets.Characters.CharacterInput
{
	public class LegacyCrossPlatformCharacterInput : MonoBehaviour, ICharacterInput
	{
		[SerializeField]
		protected LegacyCharacterInput standaloneInput;

		[SerializeField]
		protected LegacyOnScreenCharacterInput mobileInput;

		private LegacyCharacterInputBase currentInputSystem;

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

		public Vector2 lookInput
		{
			get { return currentInput.lookInput; }
		}

		public Vector2 moveInput
		{
			get { return currentInput.moveInput; }
		}

		public bool hasMovementInput
		{
			get { return currentInput.hasMovementInput; }
		}

		public Action jumpPressed
		{
			get { return currentInput.jumpPressed; }
			set { currentInput.jumpPressed = value; }
		}

		private void SetControls()
		{
			#if UNITY_ANDROID || UNITY_IOS
			SetMobileControls();
			#else
			SetStandaloneControls();
			#endif
		}

		private void SetMobileControls()
		{
			currentInputSystem = mobileInput;
			standaloneInput.gameObject.SetActive(false);
		}

		private void SetStandaloneControls()
		{
			currentInputSystem = standaloneInput;
			mobileInput.gameObject.SetActive(false);
		}
	}
}